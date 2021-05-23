using Devdeb.Network.TCP.Rpc;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Interfaces;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Models;
using System;
using System.Net;

namespace Devdeb.Network.Tests.Client
{
	public class RpcTest
	{
		static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
		static private readonly int _port = 25000;

		public void Test()
		{
			RpcClient<IClient, IServer> rpcClient = new RpcClient<IClient, IServer>(_iPAddress, _port);
			rpcClient.Start();
			rpcClient.Requestor.AddStudent(
				new StudentFm
				{
					Name = "Серафим Студентович",
					Age = 20
				},
				10
			);
			rpcClient.Requestor.DeleteStudent(Guid.Parse("6a6ccb67-df1e-478e-b649-311a9f9ec2db"));
			rpcClient.Requestor.DeleteStudent(Guid.Parse("6a6ccb67-df1e-478e-b649-311a9f9ec2db"));
			rpcClient.Requestor.DeleteStudent(Guid.Parse("6a6ccb67-df1e-478e-b649-311a9f9ec2db"));
			Console.ReadKey();
		}
	}
}
