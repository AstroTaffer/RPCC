using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using RPCC.Utils;

namespace RPCC.Comms
{
    public static class DonutsSocket
    {
        private static TcpClient _client;
        private static IPEndPoint _endPoint;
        private static NetworkStream _stream;
        private static StreamReader _streamReader;
        private static StreamWriter _streamWriter;
        public static bool IsConnected;
        private static readonly Timer PingPongTimer = new();

        static DonutsSocket()
        {
            StartDonutsPy();
            IsConnected = false;
            PingPongTimer.Elapsed += Ping;
            PingPongTimer.Interval = 1000;
        }   

        public static bool Connect()
        {   
            if (IsConnected)
            {
                Logger.AddLogEntry("WARNING Already connected to Donuts");
                return true;
            }

            _client = new TcpClient();
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            _endPoint = new IPEndPoint(IPAddress.Loopback, Settings.DonutsTcpIpPort);
            try
            {
                _client.Connect(_endPoint.Address, _endPoint.Port);
                if (_client.Connected)
                {
                    _stream = _client.GetStream();
                    _streamReader = new StreamReader(_stream, Encoding.UTF8);
                    _streamWriter = new StreamWriter(_stream, Encoding.UTF8);
                    _streamWriter.AutoFlush = true;
                    IsConnected = true;
                    // PingPongTimer.Start();
                    Logger.AddLogEntry($"Connected to Donuts {_endPoint}");
                }
                else
                {
                    Logger.AddLogEntry("Can't connect to Donuts");
                    IsConnected = false;
                }
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException)
            {
                // In case of bugs check "catch" block in Disconnect() function
                Logger.AddLogEntry($"WARNING Unable to connect to Donuts: {ex.Message}");
            }

            return IsConnected;
        }

        internal static void Disconnect()
        {
            if (!IsConnected)
            {
                Logger.AddLogEntry("WARNING Already disconnected from Donuts");
                return;
            }

            try
            {
                PingPongTimer.Stop();
                ClosePythonScript();
                _streamWriter.Close();
                _streamReader.Close();
                _stream.Close();
                _client.Close();
                _client.Dispose();
                Logger.AddLogEntry("Disconnected from Donuts");
                IsConnected = false;
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException)
            {
                // I added IOException to handle situation when MeteoDome shuts down connection from his side (i.e. when MeteoDome is closed before RPCC)
                // The proper way to do this is to ping it with some basic message, but I'm afraid that it'll shuffle the answers or load up the connection
                // In case of any bugs implement proper solution with key-response pair like "ping - pong"

                Logger.AddLogEntry($"WARNING Unable to disconnect from Donuts: {ex.Message}");
            }
        }

        private static string ExchangeMessages(string request)
        {
            if (!IsConnected)
            {
                Logger.AddLogEntry("WARNING Unable to exchange messages with Donuts: not connected to Donuts");
                // if (!Connect()) return null;
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
                Logger.AddLogEntry($"WARNING Unable to exchange messages with Donuts: {ex.Message}");
                return null;
            }
        }

        public static float[] GetGuideCorrection(string im1, string im2)
        {
            var req = $"don~{im1}~{im2}"; //Формируем запрос для донатов
            Logger.AddDebugLogEntry($"DONUTS req: {req}");
            var response = ExchangeMessages(req);
            
            if (!(response is null) && response != "")
            {
                if (response.Contains("fail"))
                {
                    Logger.AddLogEntry($"WARNING: DONUTS resp: {response}");
                    return new float[] { 0, 0, 0, 0 };
                }
                // Logger.AddDebugLogEntry($"WARNING: DONUTS resp: {response}");
                return response.Split('~').Select(float.Parse).ToArray();
            }
            Logger.AddLogEntry("WARNING Disconnecting from Donuts");
            Disconnect();
            return null;
        }
        
        public static float[] GetImageFwhm(string im)
        {
            var req = $"fwhm~{im}"; //Формируем запрос для донатов
            Logger.AddDebugLogEntry($"FWHM req: {req}");
            var response = ExchangeMessages(req);
            if (!(response is null) && response != "")
            {
                if (!response.Contains("fail")) return response.Split('~').Select(float.Parse).ToArray();
                Logger.AddLogEntry($"WARNING: FWHM {response}");
                return new float[] { 0, 0, 0, 0, 0 };
            }
            Logger.AddLogEntry("WARNING Disconnecting from Donuts");
            Disconnect();
            return null;
        }

        private static void ClosePythonScript()
        {
            _streamWriter.WriteLine("quit");
        }

        private static void StartDonutsPy()
        {
            var cwd = Directory.GetCurrentDirectory();
            var start = new ProcessStartInfo
            {
                FileName = "python.exe", //cmd is full path to python.exe
                Arguments = "\"" + cwd + "\\Guid\\DONUTS.py\"", //args is path to .py file and any cmd line args
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            Process.Start(start);
        }

        public static string PingServer()
        {
            return ExchangeMessages("ping");
        }
        
        private static void Ping(object sender, ElapsedEventArgs e)
        {
            PingServer();
        }
    }
}