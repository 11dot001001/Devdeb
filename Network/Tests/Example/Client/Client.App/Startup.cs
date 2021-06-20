using Devdeb.DependencyInjection;
using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Network.TCP.Rpc;
using System;
using System.Collections.Generic;
using Contracts.Server;
using Contracts.Client.Controllers;
using Client.App.Controllers;
using Client.App.HostedServices;
using Client.Domain;
using Devdeb.Network.TCP.Rpc.Pipelines;

namespace Client.App
{
	public class Startup : IStartup
	{
		public Type RequestorType => typeof(ServerApi);
		public Func<RequestorCollection> CreateRequestor => () => new ServerApi();

		public void AddControllers(Dictionary<Type, Type> controllerSurjection)
		{
			controllerSurjection.Add(typeof(IStudentController), typeof(StudentController));
		}
		public void ConfigureHostedServices(List<Type> hostedServices)
		{
			hostedServices.Add(typeof(MainLoopService));
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
