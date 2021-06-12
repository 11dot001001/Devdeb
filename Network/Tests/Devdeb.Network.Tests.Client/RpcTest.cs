using Devdeb.DependencyInjection;
using Devdeb.Network.TCP.Rpc;
using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Client;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Api;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Client;
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

		public async Task Test()
		{
			ServerApi serverApi = new ServerApi();

			RpcClient client = new RpcClient(_iPAddress, _port, new Startup(serverApi));
			client.Start();

			for (; ; )
			{
				new Task(() =>
				{
					var id = serverApi.StudentContoller.AddStudent(
						new StudentFm
						{
							Name = "Серафим Студентович 3",
							Age = 20
						},
						10
					).ContinueWith(id =>
					{
						serverApi.StudentContoller.GetStudent(id.Result).ContinueWith(student =>
						{
							Console.WriteLine(student.Result.Name);
						});
					});
				}).Start();
				Console.WriteLine("Free student id: " + serverApi.StudentContoller.FreeId);
				Console.WriteLine("Teacher id: " + await serverApi.TeacherContoller.AddTeacher("Марья Ивановна"));
			}

			Console.ReadKey();
		}
	}

	public class Startup : IStartup
	{
		private readonly ServerApi _serverApi;

		public Startup(ServerApi serverApi)
		{
			_serverApi = serverApi ?? throw new ArgumentNullException(nameof(serverApi));
		}

		public Type RequestorType => typeof(ServerApi);
		public Func<RequestorCollection> CreateRequestor => () => _serverApi;

		public void AddControllers(Dictionary<Type, Type> controllerSurjection)
		{
			controllerSurjection.Add(typeof(IClientController), typeof(ClientController));
		}
		public void AddServices(IServiceCollection serviceCollection) { }
	}
}
