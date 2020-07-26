using Devdeb.Network.TCP.Connection;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static Devdeb.Network.TCP.Connection.TCPConnectionPackage;

namespace Devdeb.Network.TCP
{
	public abstract class BaseTCPClient
	{
		private readonly Thread _connectionProcessing;
		private readonly IPAddress _serverIPAddress;
		private readonly int _serverPort;
		private TCPConnectionProvider _tcpConnectionProvider;

		public BaseTCPClient(IPAddress serverIPAddress, int serverPort)
		{
			_serverIPAddress = serverIPAddress ?? throw new System.ArgumentNullException(nameof(serverIPAddress));
			_serverPort = serverPort;
			_connectionProcessing = new Thread(ProcessConnections);
		}

		public IPAddress ServerIPAddress => _serverIPAddress;
		public int ServerPort => _serverPort;

		public void Start()
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Connect(new IPEndPoint(_serverIPAddress, _serverPort));
			_tcpConnectionProvider = new TCPConnectionProvider(socket);
			_connectionProcessing.Start();
		}
		public void Stop()
		{
			_tcpConnectionProvider.Close();
			_connectionProcessing.Abort();
		}

		protected virtual void SendBytes(byte[] bytes) => _tcpConnectionProvider.AddPackageToSend(new TCPConnectionPackage(bytes, CreatingBytesAction.CopyDataWithLenght));
		protected abstract void ReceiveBytes(byte[] bytes);

		private void ProcessConnections()
		{ 
			for(; ; )
			{
				_tcpConnectionProvider.SendBytes();
				_tcpConnectionProvider.ReceiveBytes();
				if (_tcpConnectionProvider.ReceivedPackagesCount != 0)
				{ 
					TCPConnectionPackage package = _tcpConnectionProvider.GetPackage();
					ReceiveBytes(package.Data);
				}
				Thread.Sleep(1);
			}
		}
	}
}