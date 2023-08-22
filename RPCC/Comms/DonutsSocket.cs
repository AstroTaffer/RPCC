using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using RPCC.Utils;

namespace RPCC.Comms
{
    public class DonutsSocket
    {
        private readonly Logger _logger;
        private readonly Settings _settings;
        internal bool isConnected;
        private IPEndPoint _endPoint;
        private TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;

        internal DonutsSocket(Logger logger, Settings settings)
        {
            StartDonutsPy();
            _logger = logger;
            _settings = settings;
            isConnected = false;
        }
        
        internal void Connect()
        {
            if (isConnected)
            {   
                _logger.AddLogEntry("WARNING Already connected to Donuts");
                return;
            }
           
            _client = new TcpClient();
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[1];
            _endPoint = new IPEndPoint(ipAddress, _settings.DonutsTcpIpPort);
            try
            {
                _client.Connect(_endPoint.Address, _endPoint.Port);
                if (_client.Connected)
                {
                    _stream = _client.GetStream();
                    _streamReader = new StreamReader(_stream, System.Text.Encoding.UTF8);
                    _streamWriter = new StreamWriter(_stream, System.Text.Encoding.UTF8);
                    _streamWriter.AutoFlush = true;
                    isConnected = true;

                    _logger.AddLogEntry($"Connected to Donuts {_endPoint}");
                }
                else
                {
                    _logger.AddLogEntry("Can't connect to Donuts");
                    isConnected = false;
                }
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException)
            {
                
                // In case of bugs check "catch" block in Disconnect() function
                _logger.AddLogEntry($"WARNING Unable to connect to Donuts {ex.Message}");
            }
        }
        internal void Disconnect()
        {
            if (!isConnected)
            {
                _logger.AddLogEntry("WARNING Already disconnected from Donuts");
                return;
            }

            try
            {
                ClosePythonScript();
                _streamWriter.Close();
                _streamReader.Close();
                _stream.Close();
                _client.Close();
                _client.Dispose();
                _logger.AddLogEntry("Disconnected from Donuts");
                isConnected = false;
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException)
            {
                // I added IOException to handle situation when MeteoDome shuts down connection from his side (i.e. when MeteoDome is closed before RPCC)
                // The proper way to do this is to ping it with some basic message, but I'm afraid that it'll shuffle the answers or load up the connection
                // In case of any bugs implement proper solution with key-response pair like "ping - pong"
                
                _logger.AddLogEntry($"WARNING Unable to disconnect from Donuts: {ex.Message}");
            }
        }
        
        internal string ExchangeMessages(string request)
        {
            if (!isConnected)
            {
                _logger.AddLogEntry("WARNING Unable to exchange messages with Donuts: not connected to Donuts");
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
                _logger.AddLogEntry($"WARNING Unable to exchange messages with Donuts: {ex.Message}");
                return null;
            }
        }

        public float[] GetGuideCorrection(string req)
        {
            
            var response =  ExchangeMessages(req);
            if (!(response is null) && response != "")
            {
                return response == "fail" ? new float[]{0, 0} : response.Split(' ').Select(float.Parse).ToArray();
            }
            _logger.AddLogEntry("WARNING Disconnecting from Donuts");
            Disconnect();
            return null;
        }

        private void ClosePythonScript()
        {
            _streamWriter.WriteLine("quit");
        }
        
        private void StartDonutsPy()
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

        public string PingServer()
        {
            return ExchangeMessages("ping");
        }
    }
}