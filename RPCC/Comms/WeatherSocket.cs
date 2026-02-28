using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using RPCC.Tasks;
using RPCC.Utils;
using Timer = System.Timers.Timer;

namespace RPCC.Comms
{
    static class WeatherSocket
    {
        private static readonly Timer MeteoTimer;
        private static TcpClient _client;
        private static IPEndPoint _endPoint;

        internal static bool IsConnected;
        private static NetworkStream _stream;
        private static StreamReader _streamReader;
        private static StreamWriter _streamWriter;
        
        // StreamReader/Writer are not thread-safe; serialize all IO through this gate.
        private static readonly SemaphoreSlim IoGate = new(1, 1);

        // Prevent overlapping timer ticks (System.Timers.Timer can re-enter).
        private static int _meteoTickRunning = 0;

        // Avoid infinite blocking on network IO.
        private const int IoTimeoutMs = 3000;

        /**
         * Valid messages:
         * sky     ---
         * sky std ---
         * ext     ---
         * ext std ---
         * see     ---
         * see ext ---
         * wind    ---
         * sun     ---
         * obs     ---
         * flat    ---
         * full    ---
         * ping    ---
         */
        static WeatherSocket()
        {
            MeteoTimer = new Timer(1000);
            // MeteoTimer.Elapsed += OnMeteoTimedEventAsync;
            MeteoTimer.AutoReset = false; // restart manually to avoid re-entrancy
            MeteoTimer.Elapsed += OnMeteoTimedEvent;
            
            IsConnected = false;
        }

        // internal static async void Connect()
        internal static async Task<bool> ConnectAsync()
        {
            if (IsConnected)
            {
                Logger.AddLogEntry("WARNING Already connected to MeteoDome");
                return true;
            }

            _client = new TcpClient();
            _endPoint = new IPEndPoint(IPAddress.Loopback, Settings.MeteoDomeTcpIpPort);
            try
            {
                await _client.ConnectAsync(_endPoint.Address, _endPoint.Port);
                if (_client.Connected)
                {
                    _stream = _client.GetStream();
                    
                    _stream.ReadTimeout = IoTimeoutMs;
                    _stream.WriteTimeout = IoTimeoutMs;
                    _streamReader = new StreamReader(_stream, Encoding.UTF8);
                    _streamWriter = new StreamWriter(_stream, Encoding.UTF8);
                    _streamWriter.AutoFlush = true;
                    IsConnected = true;
                    
                    await GetFullDataAsync().ConfigureAwait(false);
                    MeteoTimer.Start();

                    Logger.AddLogEntry($"Connected to MeteoDome {_endPoint}");
                    return true;
                }
                else
                {
                    MeteoTimer.Stop();
                    return false;
                }
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException)
            {
                // In case of bugs check "catch" block in Disconnect() function
                MeteoTimer.Stop();
                Logger.AddLogEntry($"WARNING Unable to connect to MeteoDome: {ex.Message}");
                return false;
            }
        }

        internal static void Disconnect()
        {
            if (!IsConnected)
            {
                Logger.AddLogEntry("WARNING Already disconnected from MeteoDome");
                return;
            }

            try
            {
                // MeteoTimer.Stop();
                // MeteoTimer.Close();
                
                // Mark disconnected first to prevent new IO.
                IsConnected = false;
                try { MeteoTimer.Stop(); } catch { /* ignore */ }
                
                // _streamWriter.Close();
                // _streamReader.Close();
                // _stream.Close();
                // _client.Close();
                
                // Closing the underlying socket/stream should unblock any pending ReadLine().
                try { _streamWriter?.Close(); } catch { /* ignore */ }
                try { _streamReader?.Close(); } catch { /* ignore */ }
                try { _stream?.Close(); } catch { /* ignore */ }
                try { _client?.Close(); } catch { /* ignore */ }

                Logger.AddLogEntry("Disconnected from MeteoDome");
                // IsConnected = false;
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException)
            {
                // I added IOException to handle situation when MeteoDome shuts down connection from his side (i.e. when MeteoDome is closed before RPCC)
                // The proper way to do this is to ping it with some basic message, but I'm afraid that it'll shuffle the answers or load up the connection
                // In case of any bugs implement proper solution with key-response pair like "ping - pong"

                Logger.AddLogEntry($"WARNING Unable to disconnect from MeteoDome: {ex.Message}");
            }
        }

        internal static async Task<string> ExchangeMessagesAsync(string request)
        {
            if (!IsConnected)
            {
                Logger.AddLogEntry("WARNING Unable to exchange messages with MeteoDome: not connected");
                return null;
            }

            try
            {
                // await _streamWriter.WriteLineAsync(request);
                // var response = await _streamReader.ReadLineAsync();
                // return response;
                
                await IoGate.WaitAsync().ConfigureAwait(false);
                try
                {
                    await _streamWriter.WriteLineAsync(request).ConfigureAwait(false);
                    return await _streamReader.ReadLineAsync().ConfigureAwait(false);
                }
                finally
                {
                    IoGate.Release();
                }
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException)
            {
                // In case of bugs check "catch" block in Disconnect() function
                Logger.AddLogEntry($"WARNING Unable to exchange messages with MeteoDome: {ex.Message}");
                return null;
            }
            catch (InvalidOperationException exception)
            {
                Logger.AddLogEntry(exception.Message);
                return null;
            }
        }

        internal static async Task GetFullDataAsync()
        {
            // var response = "";
            string response;
            try
            {
                response = await ExchangeMessagesAsync("full");
            }
            catch (InvalidOperationException exception)
            {
                Logger.AddLogEntry(exception.Message);
                // Disconnect();
                // Connect();
                return;
            }
            
            if (response is null)
            {
                Logger.AddLogEntry("WARNING Disconnecting from MeteoDome");
                Disconnect();
                // Connect();
            }
            else
            {
                // _collector.ParseFullData(response);
                WeatherDataCollector.ParseFullData(response);
            }
        }

        // Keep sync API for existing callers, but make it serialized too.
        internal static string ExchangeMessages(string request)
        {
            if (!IsConnected)
            {
                Logger.AddLogEntry("WARNING Unable to exchange messages with MeteoDome: not connected to MeteoDome");
                return null;
            }

            try
            {
                // _streamWriter.WriteLine(request);
                // var response = _streamReader.ReadLine();
                // return response;
                
                IoGate.Wait();
                try
                {
                    _streamWriter.WriteLine(request);
                    return _streamReader.ReadLine();
                }
                finally
                {
                    IoGate.Release();
                }
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException)
            {
                // In case of bugs check "catch" block in Disconnect() function
                Logger.AddLogEntry($"WARNING Unable to exchange messages with MeteoDome: {ex.Message}");
                return null;
            }
        }

        internal static void GetFullData()
        {
            var response = ExchangeMessages("full");
            if (response is null)
            {
                Logger.AddLogEntry("WARNING Disconnecting from MeteoDome");
                Disconnect();
            }
            else
            {
                // _collector.ParseFullData(response);
                WeatherDataCollector.ParseFullData(response);
                // Logger.AddLogEntry(response);
                //
            }
        }

        private static void OnMeteoTimedEvent(object sender, ElapsedEventArgs e)
        {
            // if (!IsConnected)
            //     MeteoTimer.Stop();
            // else
            //     GetFullData();
            //     // GetFullDataAsync();
            
            // AutoReset=false: restart manually.
            if (Interlocked.Exchange(ref _meteoTickRunning, 1) == 1)
            {
                if (IsConnected) MeteoTimer.Start();
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    if (IsConnected) await GetFullDataAsync().ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.Exchange(ref _meteoTickRunning, 0);
                    if (IsConnected) MeteoTimer.Start();
                }
            });
        }
    }

    internal static class WeatherDataCollector
    {
        public static double Amb { get; set; }
        public static double Sky { get; set; }
        public static double SkyStd { get; set; }
        public static double Extinction { get; set; }
        public static double ExtinctionStd { get; set; }
        public static double Seeing { get; set; }
        public static double SeeingExtinction { get; set; }
        public static double Wind { get; set; }
        public static double Sun { get; set; }
        public static bool Obs { get; set; }
        public static bool Flat { get; set; }

        internal static void ParseFullData(string data)
        {
            // data = "{Sky} {SkyStd} {Ext} {ExtStd} {See} {SeeExt} {Wind} {Sun} {Obs} {Flat}"
            // Example: -28.5 +.1 +10 +0 +0 -1 +.7 +92.9 False False

            var buffData = data.Split(' ');

            Sky = double.Parse(buffData[0]);
            SkyStd = double.Parse(buffData[1]);
            Extinction = double.Parse(buffData[2]);
            ExtinctionStd = double.Parse(buffData[3]);
            Seeing = double.Parse(buffData[4]);
            SeeingExtinction = double.Parse(buffData[5]);
            Wind = double.Parse(buffData[6]);
            Sun = double.Parse(buffData[7]);
            Obs = bool.Parse(buffData[8]);
            Flat = bool.Parse(buffData[9]);

            Amb = Math.Round(DbCommunicate.GetActualAmbientTemp(), 2);
            // Logger.AddLogEntry($"Amb {Amb}");
        }
    }
}