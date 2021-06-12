using Models;
using System;

namespace Client.Domain.Services.Abstractions
{
	public interface IStudentService
	{
		void HandleStudentUpdate(Guid id, StudentVm student);
	}
}
