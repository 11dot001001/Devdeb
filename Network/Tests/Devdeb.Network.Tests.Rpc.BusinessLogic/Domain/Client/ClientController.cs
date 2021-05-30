using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Client;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Models;
using System;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Client
{
	public class ClientController : IClientController
	{
		public void HandleStudentUpdate(Guid id, StudentVm student)
		{
			Console.WriteLine($"Client {id} has been updated. Name {student.Name}");
		}
	}
}
