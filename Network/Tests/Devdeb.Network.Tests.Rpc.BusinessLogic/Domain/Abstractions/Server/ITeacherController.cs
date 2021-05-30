using System;
using System.Threading.Tasks;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Server
{
	public interface ITeacherController
	{
		Task<Guid> AddTeacher(string name);
	}
}
