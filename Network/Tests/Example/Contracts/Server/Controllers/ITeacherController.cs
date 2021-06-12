using System;
using System.Threading.Tasks;

namespace Contracts.Server.Controllers
{
	public interface ITeacherController
	{
		Task<Guid> AddTeacher(string name);
	}
}
