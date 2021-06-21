using Devdeb.DependencyInjection;
using Devdeb.Network.TCP.Rpc.Controllers.Registrators;
using Devdeb.Network.TCP.Rpc.HostedServices.Registrators;
using Devdeb.Network.TCP.Rpc.Pipelines;
using Devdeb.Network.TCP.Rpc.Requestor.Registrators;

namespace Devdeb.Network.TCP.Rpc
{
	public interface IStartup
	{
		void ConfigureRequestor(IRequestorRegistrator registrator);
		void ConfigureControllers(IControllerRegistrator registrator);
		void ConfigureHostedServices(IHostedServiceRegistrator registrator);
		void ConfigureServices(IServiceCollection serviceCollection);
		void ConfigurePipeline(IPipelineBuilder pipelineBuilder);
	}
}
