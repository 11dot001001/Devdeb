using Devdeb.Network.TCP.Communication;

namespace Devdeb.Network.TCP.Rpc.Requestor.Context
{
	internal class RequestorContext : IRequestorContext
	{
		private TcpCommunication _tcpCommunication;

		public TcpCommunication TcpCommunication => _tcpCommunication;

		public void SetTcpCommunication(TcpCommunication tcpCommunication) => _tcpCommunication = tcpCommunication;
	}
}
