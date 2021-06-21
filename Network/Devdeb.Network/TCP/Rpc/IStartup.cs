using Devdeb.DependencyInjection;
using Devdeb.Network.TCP.Rpc.Controllers.Registrators;
using Devdeb.Network.TCP.Rpc.HostedServices.Registrators;
using Devdeb.Network.TCP.Rpc.Pipelines;
using Devdeb.Network.TCP.Rpc.Requestor;
using System;

namespace Devdeb.Network.TCP.Rpc
{
	public interface IStartup
	{
		Type RequestorType { get; }
		Func<RequestorCollection> CreateRequestor { get; }
		void ConfigureControllers(IControllerRegistrator registrator);
		void ConfigureHostedServices(IHostedServiceRegistrator registrator);
		void ConfigureServices(IServiceCollection serviceCollection);
		void ConfigurePipeline(IPipelineBuilder pipelineBuilder);
	}
}
