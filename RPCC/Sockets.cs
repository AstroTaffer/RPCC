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
        private bool _dome;
        private readonly bool _don;
        private bool _donRef;
        
        internal RpccSocketClient(Logger logger, string program)
        {
            _logger = logger;
            if (program == "Dome" || program == "dome" || program == "don")
            {
                _dome = true;
                Connect2Dome();
            }
            else if (program == "Donuts" || program == "donuts" || program == "don")
            {
                _don = true;
                Connect2Don();
            }
        }

        private void Connect2Dome()
        {
            _endPoint = new IPEndPoint(IPAddress.Loopback, PortDome);  // получаем адреса для запуска сокета
            try
            {
                _client = new TcpClient(_endPoint);
                _stream = _client.GetStream();
            }
            catch (SocketException)
            {
                _logger.AddLogEntry("Unable to connect to MeteoDome");
            }
        }
        private void Connect2Don()
        {
            _endPoint = new IPEndPoint(IPAddress.Loopback, PortDon);
            try
            {
                _client = new TcpClient(_endPoint);
                _stream = _client.GetStream();
            }
            catch (SocketException)
            {
                _logger.AddLogEntry("Unable to connect to DONUTS script");
            }
        }

        public bool DonutSetRef(string refPath)
        {
            if (!_don)
            {
                _logger.AddLogEntry("Socket error: donuts = false");
                return false;
            }

            if (!_client.Connected)
            {
                _logger.AddLogEntry("Socket error: client disconnected");
                return false;
            }
            var lenBytes = Encoding.UTF8.GetBytes(refPath.Length.ToString());
            _stream.Write(lenBytes, 0, lenBytes.Length);
            var pathBytes = Encoding.UTF8.GetBytes(refPath);
            _stream.Write(pathBytes, 0, pathBytes.Length);
            _donRef = true;
            return true;
        }

        public float[] DonutGetShift(string path)
        {
            var shift = new float[2];
            if (!_don)
            {
                _logger.AddLogEntry("Socket error: donuts = false");
                return shift;
            }
            if (!_client.Connected)
            {
                _logger.AddLogEntry("Socket error: client disconnected");
                return shift;
            }
            if (!_donRef)
            {
                _logger.AddLogEntry("Socket error: ref file for donuts not set");
                return shift;
            }
            var pathBytes = Encoding.UTF8.GetBytes(path);
            _stream.Write(pathBytes, 0, pathBytes.Length);
            var data = new byte[16];    
            _stream.Read(data, 0, data.Length);
            var response = Encoding.UTF8.GetString(data);
            
            if (!string.IsNullOrEmpty(response))
            {
                _logger.AddLogEntry("Donuts sifts: " + response);
                shift = response.Split(' ').Select(float.Parse).ToArray();
            }
            else
            {
                _logger.AddLogEntry("Socket error: len(response)=0");
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