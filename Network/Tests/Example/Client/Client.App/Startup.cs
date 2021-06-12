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
		public void AddHostedServices(List<Type> hostedServices)
		{
			hostedServices.Add(typeof(MainLoopService));
		}
		public void AddServices(IServiceCollection serviceCollection) 
		{
			serviceCollection.AddDomain();
		}
	}
}
