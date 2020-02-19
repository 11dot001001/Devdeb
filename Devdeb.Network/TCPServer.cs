using System;
using System.Collections.Generic;
using System.Net;

namespace Devdeb.Network
{
	public class TCPServer
	{
		private readonly IPAddress _ipAddress;
		private readonly int _port;
		private readonly int _backlog;
		private readonly TCPSocketListener _socketListener;
		private readonly Queue<TCPConnection> _connections;

		public TCPServer(IPAddress ipAddress, int port, int backlog)
		{
			_ipAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
			_port = port;
			_backlog = backlog;
			_socketListener = new TCPSocketListener(_ipAddress, _port);
			_connections = new Queue<TCPConnection>();
		}

		public void Start()
		{
			_socketListener.Start(_backlog);
		}
	}
}