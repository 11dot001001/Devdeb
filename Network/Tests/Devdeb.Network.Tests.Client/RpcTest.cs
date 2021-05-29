using Devdeb.Network.TCP.Rpc;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Interfaces;
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
			RpcClient<IClient, IServer> rpcClient = new RpcClient<IClient, IServer>(_iPAddress, _port);
			rpcClient.Start();


			for (; ; )
			{
				Task.Factory.StartNew(() =>
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
				});
				Console.WriteLine(rpcClient.Requestor.FreeId);
			}


			//for (; ; )
			//{
			//	Task.Factory.StartNew(() =>
			//	{
			//		rpcClient.Requestor.AddStudent(
			//			new StudentFm
			//			{
			//				Name = "Серафим Студентович 3",
			//				Age = 20
			//			},
			//			10
			//		).ContinueWith(x => Console.WriteLine(x.Result));
			//	});
			//	Console.WriteLine(rpcClient.Requestor.FreeId);
			//}
			Console.ReadKey();
		}
	}
}
