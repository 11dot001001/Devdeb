using System.Net;
using System.Net.Sockets;

namespace Devdeb.Network
{
	public class TCPConnectionClient
	{
		private readonly IPAddress _ipAddress;
		private readonly int _port;
		private readonly Socket _connection;

		public TCPConnectionClient(IPAddress ipAddress, int port)
		{
			_ipAddress = ipAddress;
			_port = port;
			_connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		public void Start()
		{
			_connection.Connect(new IPEndPoint(_ipAddress, _port));
		}
	}
}