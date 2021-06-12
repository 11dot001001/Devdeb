using Devdeb.Network.TCP.Communication;

namespace Devdeb.Network.TCP.Rpc.Requestor.Context
{
	public interface IRequestorContext
	{
		TcpCommunication TcpCommunication { get; }
	}
}
