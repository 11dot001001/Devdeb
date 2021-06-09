using Devdeb.Network.TCP.Rpc;
using Devdeb.Network.TCP.Rpc.Handler;
using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Client;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Server;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Client;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Server;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Devdeb.Network.Tests.Client
{
	public class RpcTest
	{
		static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
		static private readonly int _port = 25000;

		public sealed class ClientRequestors : RequestorCollection
		{
			public IStudentContoller StudentContoller { get; set; }
			public ITeacherController TeacherContoller { get; set; }
		}

		public async Task Test()
		{
			Dictionary<Type, Type> controllers = new Dictionary<Type, Type>
			{
				[typeof(IClientController)] = typeof(ClientController),
			};
			ClientRequestors requestors = new ClientRequestors();

			RpcClient client = new RpcClient(_iPAddress, _port, controllers, requestors);
			client.Start();

			for (; ; )
			{
				new Task(() =>
				{
					var id = requestors.StudentContoller.AddStudent(
						new StudentFm
						{
							Name = "Серафим Студентович 3",
							Age = 20
						},
						10
					).ContinueWith(id =>
					{
						requestors.StudentContoller.GetStudent(id.Result).ContinueWith(student =>
						{
							Console.WriteLine(student.Result.Name);
						});
					});
				}).Start();
				Console.WriteLine("Free student id: " + requestors.StudentContoller.FreeId);
				Console.WriteLine("Teacher id: " + await requestors.TeacherContoller.AddTeacher("Марья Ивановна"));
			}

			Console.ReadKey();
		}
	}
}
