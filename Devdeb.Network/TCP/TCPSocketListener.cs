using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Devdeb.Network.TCP
{
	public class TCPSocketListener
	{
		private readonly Socket _listenerSocket;
		private readonly Thread _acceptingThread;
		private readonly Queue<Socket> _acceptedSocketsQueue;

		public readonly IPAddress IPAddress;
		public readonly int Port;
		public readonly int Backlog;

		public TCPSocketListener(IPAddress ipAddress, int port, int backlog)
		{
			IPAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
			Port = port;
			Backlog = backlog;
			_listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_acceptingThread = new Thread(Accept);
			_acceptedSocketsQueue = new Queue<Socket>();
		}

		public int AcceptedConnectionsCount
		{
			get
			{
				lock (_acceptedSocketsQueue)
					return _acceptedSocketsQueue.Count;
			}
		}

		public void Start()
		{
			_listenerSocket.Bind(new IPEndPoint(IPAddress, Port));
			_listenerSocket.Listen(Backlog);
			_acceptingThread.Start();
		}
		public void Stop()
		{
			_listenerSocket.Shutdown(SocketShutdown.Both);
			_listenerSocket.Close();
			foreach (Socket socket in _acceptedSocketsQueue)
			{
				socket.Shutdown(SocketShutdown.Both);
				socket.Close();
			}
			_acceptingThread.Abort();
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