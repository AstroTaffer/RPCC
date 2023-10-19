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
    internal class WeatherSocket
    {
        private readonly Timer _meteoTimer;
        private TcpClient _client;
        private IPEndPoint _endPoint;

        internal bool _isConnected;
        private NetworkStream _stream;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;

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
        internal WeatherSocket()
        {
            // Logger = logger;
            // Settings = settings;
            // _collector = collector;

            _meteoTimer = new Timer(60000);
            _meteoTimer.Elapsed += OnMeteoTimedEventAsync;

            _isConnected = false;
        }

        internal async void Connect()
        {
            if (_isConnected)
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
                    _isConnected = true;

                    GetFullDataAsync();
                    _meteoTimer.Start();

                    Logger.AddLogEntry($"Connected to MeteoDome {_endPoint}");
                }
                else
                {
                    _meteoTimer.Stop();
                }
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException)
            {
                // In case of bugs check "catch" block in Disconnect() function
                _meteoTimer.Stop();
                Logger.AddLogEntry($"WARNING Unable to connect to MeteoDome: {ex.Message}");
            }
        }

        internal void Disconnect()
        {
            if (!_isConnected)
            {
                Logger.AddLogEntry("WARNING Already disconnected from MeteoDome");
                return;
            }

            try
            {
                _meteoTimer.Stop();
                _meteoTimer.Close();

                _streamWriter.Close();
                _streamReader.Close();
                _stream.Close();
                _client.Close();

                Logger.AddLogEntry("Disconnected from MeteoDome");
                _isConnected = false;
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException)
            {
                // I added IOException to handle situation when MeteoDome shuts down connection from his side (i.e. when MeteoDome is closed before RPCC)
                // The proper way to do this is to ping it with some basic message, but I'm afraid that it'll shuffle the answers or load up the connection
                // In case of any bugs implement proper solution with key-response pair like "ping - pong"

                Logger.AddLogEntry($"WARNING Unable to disconnect from MeteoDome: {ex.Message}");
            }
        }

        internal async Task<string> ExchangeMessagesAsync(string request)
        {
            if (!_isConnected)
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
        }

        internal async void GetFullDataAsync()
        {
            var response = await ExchangeMessagesAsync("full");
            if (response is null)
            {
                Logger.AddLogEntry("WARNING Disconnecting from MeteoDome");
                Disconnect();
            }
            else
            {
                // _collector.ParseFullData(response);
                WeatherDataCollector.ParseFullData(response);
            }
        }

        internal string ExchangeMessages(string request)
        {
            if (!_isConnected)
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

        internal void GetFullData()
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
            }
        }

        private void OnMeteoTimedEventAsync(object sender, ElapsedEventArgs e)
        {
            if (!_isConnected)
                _meteoTimer.Stop();
            else
                GetFullDataAsync();
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