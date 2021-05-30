using Devdeb.Network.TCP.Rpc;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Client;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Server;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Client;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Devdeb.Network.Tests.Client
{
	public class RpcTest
	{
		static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
		static private readonly int _port = 25000;

		public void Test()
		{
			ClientController clientImplementation = new ClientController();
			RpcClient<IClientController, IStudentContoller> rpcClient = new RpcClient<IClientController, IStudentContoller>(_iPAddress, _port, clientImplementation);
			rpcClient.Start();

			for (; ; )
			{
				new Task(() =>
				{
					var id = rpcClient.Requestor.AddStudent(
						new StudentFm
						{
							Name = "Серафим Студентович 3",
							Age = 20
						},
						10
					).ContinueWith(id =>
					{
						rpcClient.Requestor.GetStudent(id.Result).ContinueWith(student =>
						{
							Console.WriteLine(student.Result.Name);
						});
					});
				}).Start();
				Console.WriteLine(rpcClient.Requestor.FreeId);
			}

			Console.ReadKey();
		}
	}
}
