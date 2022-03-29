using Devdeb.Audio.InternetTelephony.Client.Controllers;
using Devdeb.Audio.InternetTelephony.Client.HostedServices;
using Devdeb.Audio.InternetTelephony.Contracts.Client.Controllers;
using Devdeb.Audio.InternetTelephony.Contracts.Server;
using Devdeb.DependencyInjection;
using Devdeb.Network.TCP.Rpc;
using Devdeb.Network.TCP.Rpc.Controllers.Registrators;
using Devdeb.Network.TCP.Rpc.HostedServices.Registrators;
using Devdeb.Network.TCP.Rpc.Pipelines;
using Devdeb.Network.TCP.Rpc.Requestor.Registrators;

namespace Devdeb.Audio.InternetTelephony.Client
{
	public class Startup : IStartup
	{
		public void ConfigureRequestor(IRequestorRegistrator registrator)
		{
			registrator.UseRequestor<ServerApi>();
		}
		public void ConfigureControllers(IControllerRegistrator registrator)
		{
			registrator.AddController<ICallController, CallController>();
		}
		public void ConfigureHostedServices(IHostedServiceRegistrator registrator)
		{
			registrator.AddHostedService<InputHandler>();

		}
		public void ConfigureServices(IServiceCollection serviceCollection)
		{
			serviceCollection.AddDomain();
		}
		public void ConfigurePipeline(IPipelineBuilder pipelineBuilder)
		{
			pipelineBuilder.UseControllers();
		}
	}
}
