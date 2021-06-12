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

namespace Server.App
{
	internal class Startup : IStartup
	{
		public Type RequestorType => typeof(ClientApi);
		public Func<RequestorCollection> CreateRequestor => () => new ClientApi();

		public void AddControllers(Dictionary<Type, Type> controllerSurjection)
		{
			controllerSurjection.Add(typeof(IStudentContoller), typeof(StudentContoller));
			controllerSurjection.Add(typeof(ITeacherController), typeof(TeacherController));
		}

		public void AddHostedServices(List<Type> hostedServices)
		{
			hostedServices.Add(typeof(HostedService1));
			hostedServices.Add(typeof(HostedService2));
		}

		public void AddServices(IServiceCollection serviceCollection)
		{
			serviceCollection.AddCommonServices();
			serviceCollection.AddDomain();
		}
	}
}
