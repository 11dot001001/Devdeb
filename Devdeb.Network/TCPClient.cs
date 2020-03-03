using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Devdeb.Network
{
	public abstract class TCPClient
	{
		private readonly IPAddress _ipAddress;
		private readonly int _port;
		private readonly TCPConnection _connection;
		protected readonly Thread _connectionProcessing;

		public TCPClient(IPAddress ipAddress, int port)
		{
			_ipAddress = ipAddress;
			_port = port;
			_connection = new TCPConnection(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
			_connectionProcessing = new Thread(Process);
		}

		private void Process()
		{
			for(; ; )
			{
				_connection.SendBytes();
				_connection.ReceiveBytes();
				if (_connection.TryReceivePackage(out byte[] bytes))
					ReceiveBytes(bytes);
				Thread.Sleep(1);
			}
		}

		public void Start()
		{
			_connection.Socket.Connect(new IPEndPoint(_ipAddress, _port));
			_connectionProcessing.Start();
		}


		protected void SendBytes(byte[] bytes) => _connection.SendPackage(bytes);

		protected abstract void ReceiveBytes(byte[] bytes);
	}
}