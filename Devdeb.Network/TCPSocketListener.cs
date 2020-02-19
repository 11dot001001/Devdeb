using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Devdeb.Network
{
	public class TCPSocketListener
	{
		private readonly IPAddress _ipAddress;
		private readonly int _port;
		private readonly Socket _listenerSocket;
		private readonly Thread _acceptingThread;
		private readonly Queue<Socket> _acceptedSocketsQueue;

		public TCPSocketListener(IPAddress ipAddress, int port)
		{
			_ipAddress = ipAddress;
			_port = port;
			_listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_acceptingThread = new Thread(Accept);
			_acceptedSocketsQueue = new Queue<Socket>();
		}

		public void Start(int backlog)
		{
			_listenerSocket.Bind(new IPEndPoint(_ipAddress, _port));
			_listenerSocket.Listen(backlog);
			_acceptingThread.Start();
		}

		public Socket[] UnloadAcceptedSockets()
		{
			Socket[] connections = null;
			lock (_acceptedSocketsQueue)
			{
				connections = new Socket[_acceptedSocketsQueue.Count];
				for (int i = 0; i < connections.Length; i++)
					connections[i] = _acceptedSocketsQueue.Dequeue();
			}
			return connections;
		}

		public int AcceptedConnectionsCount
		{
			get
			{
				lock (_acceptedSocketsQueue)
					return _acceptedSocketsQueue.Count;
			}
		}

		private void Accept()
		{
			for (; ; )
			{
				Socket acceptedConnection = _listenerSocket.Accept();
				lock (_acceptedSocketsQueue)
					_acceptedSocketsQueue.Enqueue(acceptedConnection);
			}
		}
	}
}