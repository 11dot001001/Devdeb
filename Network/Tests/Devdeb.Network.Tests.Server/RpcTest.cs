using Devdeb.Network.TCP.Rpc;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Client;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Server;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Server;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Models;
using System;
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
			ServerStudentContoller serverImplementation = new ServerStudentContoller();
			RpcServer<IStudentContoller, IClientController> server = new RpcServer<IStudentContoller, IClientController>(_iPAddress, _port, _backlog, serverImplementation);
			server.Test = x => x.HandleStudentUpdate(Guid.NewGuid(), new StudentVm { Name = "1212" });
			server.Start();
			Console.ReadKey();
		}
	}
}
