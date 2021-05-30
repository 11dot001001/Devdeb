using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Server;
using System;
using System.Threading.Tasks;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Server
{
	public class ServerTeacherController : ITeacherController
	{
		public Task<Guid> AddTeacher(string name)
		{
			Guid teacherId = Guid.NewGuid();
			Console.WriteLine($"The teacher {name} was added with id {teacherId}.");
			return Task.FromResult(teacherId);
		}
	}
}
