using Devdeb.Network.Tests.Rpc.BusinessLogic.Models;
using System;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Client
{
	public interface IClientController
	{
		void HandleStudentUpdate(Guid id, StudentVm student);
	}
}
