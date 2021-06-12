using Contracts.Server;
using Devdeb.Network.TCP.Rpc;
using Models;
using System;
using System.Threading.Tasks;

namespace Client.App.HostedServices
{
	public class MainLoopService : IHostedService
	{
		private readonly ServerApi _serverApi;

		public MainLoopService(ServerApi serverApi)
		{
			_serverApi = serverApi ?? throw new ArgumentNullException(nameof(serverApi));
		}

		public async Task StartAsync()
		{
			for (; ; )
			{
				new Task(() =>
				{
					var id = _serverApi.StudentContoller.AddStudent(
						new StudentFm
						{
							Name = "Серафим Студентович 3",
							Age = 20
						},
						10
					).ContinueWith(id =>
					{
						_serverApi.StudentContoller.GetStudent(id.Result).ContinueWith(student =>
						{
							Console.WriteLine(student.Result.Name);
						});
					});
				}).Start();
				Console.WriteLine("Free student id: " + _serverApi.StudentContoller.FreeId);
				Console.WriteLine("Teacher id: " + await _serverApi.TeacherContoller.AddTeacher("Марья Ивановна"));
			}
		}

		public Task StopAsync() => throw new NotImplementedException();
	}
}
