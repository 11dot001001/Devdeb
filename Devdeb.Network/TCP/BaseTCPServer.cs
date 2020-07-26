using Devdeb.Network.Connection;
using Devdeb.Network.TCP.Connection;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static Devdeb.Network.TCP.Connection.TCPConnectionPackage;

namespace Devdeb.Network.TCP
{
	public abstract class BaseTCPServer
	{
		private readonly Queue<TCPConnectionProvider> _connections;
		private readonly Queue<TCPConnectionProvider> _connectionsWithPackages;
		private readonly TCPSocketListener _tcpSocketListener;
		private readonly Thread _connectionsProcessing;
		private readonly Thread _recivedPackagesProcessing;

		protected BaseTCPServer(IPAddress iPAddress, int port, int backlog)
		{
			_tcpSocketListener = new TCPSocketListener(iPAddress, port, backlog);
			_connections = new Queue<TCPConnectionProvider>();
			_connectionsWithPackages = new Queue<TCPConnectionProvider>();
			_connectionsProcessing = new Thread(ProcessConnections);
			_recivedPackagesProcessing = new Thread(ProcessRecivedPackages);
		}

		protected Queue<TCPConnectionProvider> Connections => _connections;
		public IPAddress IPAddress => _tcpSocketListener.IPAddress;
		public int Port => _tcpSocketListener.Port;
		public int Backlog => _tcpSocketListener.Backlog;

		public virtual void Start()
		{
			_tcpSocketListener.Start();
			_connectionsProcessing.Start();
			_recivedPackagesProcessing.Start();
		}
		public virtual void Stop()
		{
			_tcpSocketListener.Stop();
			foreach (TCPConnectionProvider tcpConnectionProvider in _connections)
			{
				tcpConnectionProvider.Connection.Shutdown(SocketShutdown.Both); 
				tcpConnectionProvider.Connection.Close(); 
			}
			_connectionsProcessing.Abort();
			_recivedPackagesProcessing.Abort();
		}

		protected virtual void SendBytes(TCPConnectionProvider connectionProvider, byte[] buffer) => connectionProvider.AddPackageToSend(new TCPConnectionPackage(ConnectionPackageType.User, buffer, CreatingBytesAction.CopyData));
		protected abstract void ReceiveBytes(TCPConnectionProvider connectionProvider, byte[] buffer);

		private void ProcessConnections()
		{
			for (; ; )
			{
				if (_tcpSocketListener.AcceptedConnectionsCount != 0)
					foreach (TCPConnectionProvider acceptedConnection in _tcpSocketListener.UnloadAcceptedSockets().Select(socket => new TCPConnectionProvider(socket)))
						_connections.Enqueue(acceptedConnection);
				if (_connections.Count == 0)
				{
					Thread.Sleep(1);
					continue;
				}
				TCPConnectionProvider connectionProvider = _connections.Dequeue();
				bool inPackagesProcesing = connectionProvider.ReceivedPackagesCount != 0;
				connectionProvider.SendBytes();
				connectionProvider.ReceiveBytes();
				if (connectionProvider.ReceivedPackagesCount != 0 && !inPackagesProcesing)
					_connectionsWithPackages.Enqueue(connectionProvider);
				_connections.Enqueue(connectionProvider);
			}
		}
		private void ProcessRecivedPackages()
		{
			for (; ; )
			{
				if (_connectionsWithPackages.Count == 0)
				{
					Thread.Sleep(1);
					continue;
				}
				TCPConnectionProvider tcpConnectionProvider = _connectionsWithPackages.Dequeue();
				if (tcpConnectionProvider.ReceivedPackagesCount == 0)
					continue;
				TCPConnectionPackage package = tcpConnectionProvider.GetPackage();
				ReceiveBytes(tcpConnectionProvider, package.Data);
				if (tcpConnectionProvider.ReceivedPackagesCount == 0)
					continue;
				_connectionsWithPackages.Enqueue(tcpConnectionProvider);
			}
		}
	}
}