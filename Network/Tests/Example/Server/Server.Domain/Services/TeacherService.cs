using System.Threading.Tasks;
using System;
using Server.Domain.Services.Abstractions;

namespace Server.Domain.Services
{
	internal class TeacherService : ITeacherService
	{
		public Task<Guid> AddTeacher(string name)
		{
			Guid teacherId = Guid.NewGuid();
			Console.WriteLine($"The teacher {name} was added with id {teacherId}.");
			return Task.FromResult(teacherId);
		}
	}
}
