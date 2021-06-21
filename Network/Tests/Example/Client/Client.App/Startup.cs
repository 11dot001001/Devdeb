using Devdeb.DependencyInjection;
using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Network.TCP.Rpc;
using System;
using Contracts.Server;
using Contracts.Client.Controllers;
using Client.App.Controllers;
using Client.App.HostedServices;
using Client.Domain;
using Devdeb.Network.TCP.Rpc.Pipelines;
using Devdeb.Network.TCP.Rpc.Controllers.Registrators;
using Devdeb.Network.TCP.Rpc.HostedServices.Registrators;

namespace Client.App
{
	public class Startup : IStartup
	{
		public Type RequestorType => typeof(ServerApi);
		public Func<RequestorCollection> CreateRequestor => () => new ServerApi();

		public void ConfigureControllers(IControllerRegistrator registrator)
		{
			registrator.AddController<IStudentController, StudentController>();
		}
		public void ConfigureHostedServices(IHostedServiceRegistrator registrator)
		{
			registrator.AddHostedService<MainLoopService>();
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
