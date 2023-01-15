using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;


namespace RPCC
{
    internal class RpccSocketClient
    {
        private IPEndPoint _mdEndPoint;
        private Socket _mdSocket;
        private readonly Logger logger;

        internal RpccSocketClient(Logger logger)
        {
            this.logger = logger;
            _mdEndPoint = new IPEndPoint(IPAddress.Loopback, 8085);
            _mdSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _mdSocket.Connect(_mdEndPoint);
            }
            catch (SocketException)
            {
                this.logger.AddLogEntry("Unable to connect to MeteoDome");
            }
        }

        internal string GetPlaceholder()
        {
            return "You recieved a message";
        }

        internal void DisconnectAll()
        {
            _mdSocket.Shutdown(SocketShutdown.Both);
            _mdSocket.Close();
        }
    }
}
