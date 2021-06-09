using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Rpc.Communication;
using IServiceProvider = Devdeb.DependencyInjection.IServiceProvider;

namespace Devdeb.Network.TCP.Rpc.Handler
{
	public interface IControllerHandler
	{
		void HandleRequest(
			IServiceProvider serviceProvider,
			TcpCommunication tcpCommunication,
			CommunicationMeta meta,
			byte[] buffer,
			int offset
		);
	}
}
