using Models;
using System;
using System.Threading.Tasks;

namespace Server.Domain.Services.Abstractions
{
	public interface IStudentService
	{
		Task<Guid> AddStudent(StudentFm studentFm, int testValue);
		Task<StudentVm> GetStudent(Guid id);
		void DeleteStudent(Guid id);
		int FreeId { get; }
	}
}
