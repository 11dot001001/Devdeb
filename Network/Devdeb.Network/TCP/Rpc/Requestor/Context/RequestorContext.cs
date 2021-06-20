using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Rpc.Communication;

namespace Devdeb.Network.TCP.Rpc.Requestor.Context
{
	internal class RequestorContext : IRequestorContext
	{
		private TcpCommunication _tcpCommunication;
		private CommunicationMeta _communicationMeta;
		private byte[] _data;

		public TcpCommunication TcpCommunication => _tcpCommunication;
		public CommunicationMeta CommunicationMeta => _communicationMeta;

		public byte[] Data => _data;

		internal void SetTcpCommunication(TcpCommunication tcpCommunication) => _tcpCommunication = tcpCommunication;
		internal void SetCommunicationMeta(CommunicationMeta communicationMeta) => _communicationMeta = communicationMeta;
		internal void SetData(byte[] data) => _data = data;
	}
}
