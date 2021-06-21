using Devdeb.DependencyInjection;
using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Network.TCP.Rpc;
using System;
using System.Collections.Generic;
using Contracts.Client;
using Contracts.Server.Controllers;
using Server.App.Controllers;
using Server.App.HostedServices;
using Common;
using Server.Domain;
using Devdeb.Network.TCP.Rpc.Pipelines;
using Devdeb.Network.TCP.Rpc.Controllers.Registrators;
using Devdeb.Network.TCP.Rpc.HostedServices.Registrators;

namespace Server.App
{
	internal class Startup : IStartup
	{
		public Type RequestorType => typeof(ClientApi);
		public Func<RequestorCollection> CreateRequestor => () => new ClientApi();

		public void ConfigureControllers(IControllerRegistrator registrator)
		{
			registrator.AddController<IStudentContoller, StudentContoller>();
			registrator.AddController<ITeacherController, TeacherController>();
		}
		public void ConfigureHostedServices(IHostedServiceRegistrator registrator)
		{
			registrator.AddHostedService<HostedService1>();
			registrator.AddHostedService<HostedService2>();
		}
		public void ConfigureServices(IServiceCollection serviceCollection)
		{
			serviceCollection.AddCommonServices();
			serviceCollection.AddDomain();
		}
		public void ConfigurePipeline(IPipelineBuilder pipelineBuilder)
		{
			pipelineBuilder.UseRequestLogger();
			pipelineBuilder.UseControllers();
		}
	}
}
