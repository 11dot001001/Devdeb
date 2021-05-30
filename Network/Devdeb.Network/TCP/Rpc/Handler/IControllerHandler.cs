using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Rpc.Communication;

namespace Devdeb.Network.TCP.Rpc.Handler
{
	public interface IControllerHandler
	{
		void HandleRequest(TcpCommunication tcpCommunication, CommunicationMeta meta, byte[] buffer, int offset);
	}
}
