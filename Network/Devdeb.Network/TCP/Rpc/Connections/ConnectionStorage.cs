using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Rpc.Requestor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Devdeb.Network.TCP.Rpc.Connections
{
	internal class ConnectionStorage : IConnectionStorage
	{
		private readonly ConcurrentDictionary<TcpCommunication, Connection> _connections;

		public ConnectionStorage() => _connections = new ConcurrentDictionary<TcpCommunication, Connection>();

		public Connection Add(TcpCommunication tcpCommunication, RequestorCollection requestorCollection)
		{
			Connection connection = new Connection(tcpCommunication, requestorCollection);
			if (!_connections.TryAdd(tcpCommunication, connection))
				throw new Exception($"TcpCommunication {tcpCommunication.Socket.RemoteEndPoint} was added.");
			return connection;
		}
		public Connection Get(TcpCommunication tcpCommunication) => _connections.GetValueOrDefault(tcpCommunication);
		public void Remove(TcpCommunication tcpCommunication)
		{
			_connections.Remove(tcpCommunication, out Connection connection);
			connection.Close();
		}
		public IEnumerable<Connection> GetAll() => _connections.Values.ToArray();
	}
}
