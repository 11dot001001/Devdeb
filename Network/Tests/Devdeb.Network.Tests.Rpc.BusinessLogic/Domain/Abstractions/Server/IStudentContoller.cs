using Devdeb.Network.Tests.Rpc.BusinessLogic.Models;
using System;
using System.Threading.Tasks;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Server
{
	public interface IStudentContoller
	{
		Task<Guid> AddStudent(StudentFm studentFm, int testValue);
		Task<StudentVm> GetStudent(Guid id);
		void DeleteStudent(Guid id);
		int FreeId { get; }
	}
}
