using Devdeb.DependencyInjection;
using Devdeb.Network.TCP.Rpc;
using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Server;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Api;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Server;
using System;
using System.Collections.Generic;
using System.Net;

namespace Devdeb.Network.Tests.Server
{
	public class RpcTest
	{
		static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
		static private readonly int _port = 25000;
		static private readonly int _backlog = 1;

		public void Test()
		{
			RpcServer server = new RpcServer(_iPAddress, _port, _backlog, new Startup());
			server.Start();
		}
	}

	public class Startup : IStartup
	{
		public Type RequestorType => typeof(ClientApi);
		public Func<RequestorCollection> CreateRequestor => () => new ClientApi();

		public void AddControllers(Dictionary<Type, Type> controllerSurjection)
		{
			controllerSurjection.Add(typeof(IStudentContoller), typeof(ServerStudentContoller));
			controllerSurjection.Add(typeof(ITeacherController), typeof(ServerTeacherController));
		}
		public void AddServices(IServiceCollection serviceCollection) => serviceCollection.AddDomain();
	}
}
