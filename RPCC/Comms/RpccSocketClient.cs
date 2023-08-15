using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using RPCC.Utils;

namespace RPCC.Comms
{
    internal class RpccSocketClient
    {
        private readonly Logger _logger;
        private readonly Settings _settings;
        private IPEndPoint _endPoint;
        private TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private readonly bool _isDome;
        private readonly bool _isDon;
        private readonly bool _isSte;
        private bool _donRef;

        public RpccSocketClient(Logger logger, Settings settings, string program)
        {
            _logger = logger;
            _settings = settings;
            if (program == "Dome" || program == "dome" || program == "dom")
            {
                _isDome = true;
            }
            else if (program == "Donuts" || program == "donuts" || program == "don")
            {
                _isDon = true;
            }
            else if (program == "SiTechExe" || program == "sitechexe" || program == "ste")
            {
                _isSte = true;
            }
            else
            {
                logger.AddLogEntry("Socket warning: no program selected");
            }
        }

        #region General
        public bool Connect()
        {
            _client = new TcpClient();
            if (_isDome)
            {
                _endPoint = new IPEndPoint(IPAddress.Loopback, _settings.MeteoDomeTcpIpPort);  // получаем адреса для запуска сокета
            }
            else if (_isDon)
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[1];  //2
                _endPoint = new IPEndPoint(ipAddress, _settings.DonutsTcpIpPort);
                // _endPoint = new IPEndPoint(IPAddress, PortDon);
            }
            else if (_isSte)
            {
                _endPoint = new IPEndPoint(IPAddress.Loopback, _settings.SiTechExeTcpIpPort);
            }
            else
            {
                _logger.AddLogEntry("Socket error: unable to connect, no program selected");
                return false;
            }
            try
            {
                _client.Connect(_endPoint);
                if (_client.Connected)
                {
                    _stream = _client.GetStream();
                    _reader = new StreamReader(_stream);
                    _writer = new StreamWriter(_stream);
                    _writer.AutoFlush = true;
                    _logger.AddLogEntry($"Connected to {_client.Client.RemoteEndPoint}");
                    return true;
                }
            }
            catch (SocketException ex)
            {
                _logger.AddLogEntry(ex.Message);
                if (_isDome)
                {
                    _logger.AddLogEntry($"Socket error: unable to connect to MeteoDome");
                }
                else if (_isDon)
                {
                    _logger.AddLogEntry($"Socket error: unable to connect to DONUTS script");
                }
                else if (_isSte)
                {
                    _logger.AddLogEntry($"Socket error: unable to connect to SiTechExe");
                }
                
            }

            return false;
            // return _client.Connected;
        }

        internal void DisconnectAll()
        {
            try
            {
                if (_isDome)
                {
                    _writer.WriteLine("stop");  //HACK: tcpclient problem
                }

                if (_isDon)
                {
                    _stream.Write(Encoding.UTF8.GetBytes("0"), 0, Encoding.UTF8.GetBytes("0").Length);
                }
                if (_isSte)
                {
                    //TODO: send "CloseMe" message to make the server close the socket
                }
                _stream.Close();
                _client.Close();
                _client.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // throw;
            }

        }
        #endregion

        #region MeteoDome
        /**
         * commands:
         *      sky
         *      sky std
         *      extinction
         *      extinction std
         *      seeing
         *      seeing_extinction
         *      wind 
         *      sun [return Sun zenith distance (degree)]
         *      obs [return bool observe]
         *      flat [return bool flat]
         */
        public string DomeGetData(string msg)
        {
            if (!_isDome)
            {
                _logger.AddLogEntry("Socket dome error: dome = false");
                return "";
            }

            if (!_client.Connected)
            {
                _logger.AddLogEntry("Socket dome error: client disconnected");
                return "";
            }
            try
            {
                _writer.WriteLine(msg);

                // получаем ответ

                var data = _reader.ReadLine();
                // _logger.AddLogEntry(data);
                // var response = Encoding.UTF8.GetString(data);
                if (string.IsNullOrEmpty(data))
                {
                    _logger.AddLogEntry("Socket dome error: len(response)=0");
                }
                return data;
            }
            catch (SocketException ex)
            {
                _logger.AddLogEntry(ex.Message);
                return "";
            }
        }
        #endregion

        #region DONUTS
        public bool DonutSetRef(string refPath)
        {
            if (!_isDon)
            {
                _logger.AddLogEntry("Socket donuts error: donuts = false");
                return false;
            }

            if (!_client.Connected)
            {
                _logger.AddLogEntry("Socket donuts error: client disconnected");
                return false;
            }

            try
            {
                var lenBytes = Encoding.UTF8.GetBytes(refPath.Length.ToString());
                _stream.Write(lenBytes, 0, lenBytes.Length);
                var pathBytes = Encoding.UTF8.GetBytes(refPath);
                _stream.Write(pathBytes, 0, pathBytes.Length);
            }
            catch (SocketException ex)
            {
                _logger.AddLogEntry(ex.Message);
                return false;
            }
            _donRef = true;
            return true;
        }

        public float[] DonutGetShift(string path)
        {
            var shift = new float[2];
            if (!_isDon)
            {
                _logger.AddLogEntry("Socket donuts error: donuts = false");
                return shift;
            }
            if (!_client.Connected)
            {
                _logger.AddLogEntry("Socket donuts error: client disconnected");
                return shift;
            }
            if (!_donRef)
            {
                _logger.AddLogEntry("Socket donuts error: ref file for donuts not set");
                return shift;
            }
            var data = new byte[16];
            try
            {
                var pathBytes = Encoding.UTF8.GetBytes(path);
                _stream.Write(pathBytes, 0, pathBytes.Length);
                _stream.Read(data, 0, data.Length);
            }
            catch (SocketException ex)
            {
                _logger.AddLogEntry(ex.Message);
                return shift;
            }

            var response = Encoding.UTF8.GetString(data);

            if (!string.IsNullOrEmpty(response))
            {
                _logger.AddLogEntry("Donuts sifts: " + response);
                shift = response.Split(' ').Select(float.Parse).ToArray();
            }
            else
            {
                _logger.AddLogEntry("Socket donuts error: len(response)=0");
            }
            return shift;
        }
        #endregion

        #region SiTechExe
        #endregion
    }
}