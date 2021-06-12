using System;
using System.Threading.Tasks;

namespace Server.Domain.Services.Abstractions
{
	public interface ITeacherService
	{
		Task<Guid> AddTeacher(string name);
	}
}
