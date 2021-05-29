using Devdeb.Network.TCP.Rpc;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Interfaces;
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
			ServerImplementation serverImplementation = new ServerImplementation();
			RpcServer<IServer> server = new RpcServer<IServer>(_iPAddress, _port, _backlog, serverImplementation);
			server.Start();
			Console.ReadKey();
		}
	}
}
