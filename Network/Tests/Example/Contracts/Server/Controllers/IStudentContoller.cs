using Models;
using System;
using System.Threading.Tasks;

namespace Contracts.Server.Controllers
{
	public interface IStudentContoller
	{
		Task<Guid> AddStudent(StudentFm studentFm, int testValue);
		Task<StudentVm> GetStudent(Guid id);
		void DeleteStudent(Guid id);
		int FreeId { get; }
	}
}
