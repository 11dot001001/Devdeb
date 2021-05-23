using Devdeb.Network.Tests.Rpc.BusinessLogic.Models;
using System;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Interfaces
{
	public interface IClient
	{
		void HandleStudentUpdate(Guid id, StudentVm student);
	}
}
