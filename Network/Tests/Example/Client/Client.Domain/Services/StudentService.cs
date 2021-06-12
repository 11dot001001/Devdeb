using Client.Domain.Services.Abstractions;
using Contracts.Server;
using Models;
using System;

namespace Client.Domain.Services
{
	internal class StudentService : IStudentService
	{
		private readonly ServerApi _serverApi;

		public StudentService(ServerApi serverApi)
		{
			_serverApi = serverApi ?? throw new ArgumentNullException(nameof(serverApi));
		}

		public void HandleStudentUpdate(Guid id, StudentVm student)
		{
			Console.WriteLine($"Client {id} has been updated. Name {student.Name}");
		}
	}
}
