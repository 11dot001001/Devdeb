using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Rpc.Requestor;
using System.Collections.Generic;

namespace Devdeb.Network.TCP.Rpc.Connections
{
	public interface IConnectionStorage
	{
		Connection Add(TcpCommunication tcpCommunication, RequestorCollection requestorCollection);
		Connection Get(TcpCommunication tcpCommunication);
		void Remove(TcpCommunication tcpCommunication);
		IEnumerable<Connection> GetAll();
	}
}
