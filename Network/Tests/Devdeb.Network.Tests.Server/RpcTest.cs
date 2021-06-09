using Devdeb.Network.TCP.Rpc;
using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Client;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Server;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Server;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Models;
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

		public sealed class ServerRequestors : RequestorCollection
		{
			public IClientController ClientController { get; set; }
		}

		public void Test()
		{
			Dictionary<Type, Type> controllers = new Dictionary<Type, Type>
			{
				[typeof(IStudentContoller)] = typeof(ServerStudentContoller),
				[typeof(ITeacherController)] = typeof(ServerTeacherController)
			};

			RpcServer server = new RpcServer(_iPAddress, _port, _backlog, controllers, () => new ServerRequestors(), x => x.AddDomain());

			server.TestClientRequest = x => ((ServerRequestors)x).ClientController.HandleStudentUpdate(
				Guid.NewGuid(),
				new StudentVm { Name = "maria ivanovna" }
			);
			server.Start();

			Console.ReadKey();
		}
	}
}
