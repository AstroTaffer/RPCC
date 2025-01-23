using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using RPCC.Utils;

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
            MeteoTimer.Elapsed += OnMeteoTimedEventAsync;
            IsConnected = false;
        }

        internal static async void Connect()
        {
            if (IsConnected)
            {
                Logger.AddLogEntry("WARNING Already connected to MeteoDome");
                return;
            }

            _client = new TcpClient();
            _endPoint = new IPEndPoint(IPAddress.Loopback, Settings.MeteoDomeTcpIpPort);
            try
            {
                await _client.ConnectAsync(_endPoint.Address, _endPoint.Port);
                if (_client.Connected)
                {
                    _stream = _client.GetStream();
                    _streamReader = new StreamReader(_stream, Encoding.UTF8);
                    _streamWriter = new StreamWriter(_stream, Encoding.UTF8);
                    _streamWriter.AutoFlush = true;
                    IsConnected = true;

                    GetFullData();
                    // GetFullDataAsync();
                    MeteoTimer.Start();

                    Logger.AddLogEntry($"Connected to MeteoDome {_endPoint}");
                }
                else
                {
                    MeteoTimer.Stop();
                }
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException)
            {
                // In case of bugs check "catch" block in Disconnect() function
                MeteoTimer.Stop();
                Logger.AddLogEntry($"WARNING Unable to connect to MeteoDome: {ex.Message}");
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
                MeteoTimer.Stop();
                MeteoTimer.Close();

                _streamWriter.Close();
                _streamReader.Close();
                _stream.Close();
                _client.Close();

                Logger.AddLogEntry("Disconnected from MeteoDome");
                IsConnected = false;
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
                await _streamWriter.WriteLineAsync(request);
                var response = await _streamReader.ReadLineAsync();
                return response;
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

        internal static async void GetFullDataAsync()
        {
            var response = "";
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

        internal static string ExchangeMessages(string request)
        {
            if (!IsConnected)
            {
                Logger.AddLogEntry("WARNING Unable to exchange messages with MeteoDome: not connected to MeteoDome");
                return null;
            }

            try
            {
                _streamWriter.WriteLine(request);
                var response = _streamReader.ReadLine();
                return response;
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
            }
        }

        private static void OnMeteoTimedEventAsync(object sender, ElapsedEventArgs e)
        {
            if (!IsConnected)
                MeteoTimer.Stop();
            else
                GetFullData();
                // GetFullDataAsync();
        }
    }

    internal static class WeatherDataCollector
    {
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
        }
    }
}