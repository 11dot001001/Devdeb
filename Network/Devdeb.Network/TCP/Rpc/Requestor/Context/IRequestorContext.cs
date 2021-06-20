using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Rpc.Communication;

namespace Devdeb.Network.TCP.Rpc.Requestor.Context
{
	public interface IRequestorContext
	{
		TcpCommunication TcpCommunication { get; }
		CommunicationMeta CommunicationMeta { get; }
		byte[] Data { get; }
	}
}
