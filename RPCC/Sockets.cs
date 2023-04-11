using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RPCC
{
    internal class RpccSocketClient
    {
        private readonly Logger _logger;
        private IPEndPoint _endPoint;
        private TcpClient _client;
        private NetworkStream _stream;
        private const int PortDome = 8085;
        private const int PortDon = 3030;
        private readonly bool _dome;
        private readonly bool _don;
        private bool _donRef;
        
        internal RpccSocketClient(Logger logger, string program)
        {
            _logger = logger;
            if (program == "Dome" || program == "dome" || program == "dom")
            {
                _dome = true;
            }
            else if (program == "Donuts" || program == "donuts" || program == "don")
            {
                _don = true;
            }
            else
            {
                logger.AddLogEntry("Socket warning: no program selected");
            }
        }

        public bool Connect()
        {
            if (_dome)
            {
                _endPoint = new IPEndPoint(IPAddress.Loopback, PortDome);  // получаем адреса для запуска сокета
            }
            else if (_don)
            {
                _endPoint = new IPEndPoint(IPAddress.Loopback, PortDon);
            }
            else
            {
                _logger.AddLogEntry("Socket error: unable to connect, no program selected");
                return false;
            }
            try
            {
                _client = new TcpClient(_endPoint);
                _stream = _client.GetStream();
            }
            catch (SocketException ex)
            {
                _logger.AddLogEntry(ex.Message);
                _logger.AddLogEntry($"Socket error: unable to connect to {(_dome ? "MeteoDome" : "DONUTS script")}");
            }

            return _client.Connected;
        }

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
            if (!_dome)
            {
                _logger.AddLogEntry("Socket error: dome = false");
                return "";
            }

            if (!_client.Connected)
            {
                _logger.AddLogEntry("Socket error: client disconnected");
                return "";
            }
            try
            {
                // var message = "get sky";
                var data = Encoding.Unicode.GetBytes(msg);
                _stream.WriteAsync(data, 0, data.Length);
                
                // получаем ответ
                data = new byte[256]; // буфер для ответа
                _stream.ReadAsync(data, 0, data.Length);
                var response = Encoding.UTF8.GetString(data);
                if (string.IsNullOrEmpty(response))
                {
                    _logger.AddLogEntry("Socket dome error: len(response)=0");
                }
                return response;
            }
            catch (SocketException ex)
            {
                _logger.AddLogEntry(ex.Message);
                return "";
            }
        }

        public bool DonutSetRef(string refPath)
        {
            if (!_don)
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
                _stream.WriteAsync(lenBytes, 0, lenBytes.Length);
                var pathBytes = Encoding.UTF8.GetBytes(refPath);
                _stream.WriteAsync(pathBytes, 0, pathBytes.Length);
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
            if (!_don)
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
                _stream.WriteAsync(pathBytes, 0, pathBytes.Length);
                _stream.ReadAsync(data, 0, data.Length);
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

        internal void DisconnectAll()
        {
            _stream.Close();
            _client.Close();
        }
    }
}