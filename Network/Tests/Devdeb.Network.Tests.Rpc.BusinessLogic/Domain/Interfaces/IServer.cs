using Devdeb.Network.Tests.Rpc.BusinessLogic.Models;
using System;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Interfaces
{
	public interface IServer
	{
		void AddStudent(StudentFm studentFm, int testValue);
		StudentVm GetStudent(Guid id);
		void DeleteStudent(Guid id);
		int FreeId { get; }
	}
}
