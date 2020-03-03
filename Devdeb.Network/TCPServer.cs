using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace Devdeb.Network
{
	public abstract class TCPServer
	{
		private readonly IPAddress _ipAddress;
		private readonly int _port;
		private readonly int _backlog;
		private readonly TCPSocketListener _socketListener;
		private readonly Thread _connectionProcessing;
		protected readonly Queue<TCPConnection> _connections;

		public TCPServer(IPAddress ipAddress, int port, int backlog)
		{
			_ipAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
			_port = port;
			_backlog = backlog;
			_socketListener = new TCPSocketListener(_ipAddress, _port);
			_connections = new Queue<TCPConnection>();
			_connectionProcessing = new Thread(Process);
		}
		private void Process()
		{
			for (; ; )
			{
				if(_socketListener.AcceptedConnectionsCount != 0)
					foreach (TCPConnection tcpConnection in _socketListener.UnloadAcceptedSockets().Select(socket => new TCPConnection(socket)))
						_connections.Enqueue(tcpConnection);
				if (_connections.Count == 0)
				{
					Thread.Sleep(1);
					continue;
				}
				TCPConnection connection = _connections.Dequeue();
				connection.SendBytes();
				connection.ReceiveBytes();
				if(connection.TryReceivePackage(out byte[] bytes))
					ReceiveBytes(connection, bytes);
				_connections.Enqueue(connection);
				Thread.Sleep(1);
			}
		}

		protected void SendBytes(TCPConnection connection, byte[] bytes) => connection.SendPackage(bytes);
		protected abstract void ReceiveBytes(TCPConnection connection, byte[] bytes);

		public void Start()
		{
			_socketListener.Start(_backlog);
			_connectionProcessing.Start();
		}
	}
}