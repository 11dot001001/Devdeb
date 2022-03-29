using Devdeb.Audio.InternetTelephony.Contracts.Client;
using Devdeb.Audio.InternetTelephony.Contracts.Server.Controllers;
using Devdeb.Audio.InternetTelephony.Server.Controllers;
using Devdeb.DependencyInjection;
using Devdeb.Network.TCP.Rpc;
using Devdeb.Network.TCP.Rpc.Controllers.Registrators;
using Devdeb.Network.TCP.Rpc.HostedServices.Registrators;
using Devdeb.Network.TCP.Rpc.Pipelines;
using Devdeb.Network.TCP.Rpc.Requestor.Registrators;

namespace Devdeb.Audio.InternetTelephony.Server
{
	internal class Startup : IStartup
	{
		public void ConfigureRequestor(IRequestorRegistrator registrator)
		{
			registrator.UseRequestor<ClientApi>();
		}
		public void ConfigureControllers(IControllerRegistrator registrator)
		{
			registrator.AddController<ICallController, CallController>();
			registrator.AddController<IUserController, UserController>();
		}
		public void ConfigureHostedServices(IHostedServiceRegistrator registrator)
		{
		}
		public void ConfigureServices(IServiceCollection serviceCollection)
		{
			serviceCollection.AddDomain();
		}
		public void ConfigurePipeline(IPipelineBuilder pipelineBuilder)
		{
			pipelineBuilder.UseRequestLogger();
			pipelineBuilder.UseControllers();
		}
	}
}
