using Devdeb.Network.TCP.Rpc.Communication;

namespace Devdeb.Network.TCP.Rpc.Requestor
{
	public interface IRequestor
	{
		void HandleResponse(CommunicationMeta meta, byte[] buffer, int offset);
	}
}
