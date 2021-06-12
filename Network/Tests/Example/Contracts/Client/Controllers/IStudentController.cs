using Models;
using System;

namespace Contracts.Client.Controllers
{
	public interface IStudentController
	{
		void HandleStudentUpdate(Guid id, StudentVm student);
	}
}
