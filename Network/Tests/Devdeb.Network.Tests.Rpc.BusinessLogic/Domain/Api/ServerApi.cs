using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Server;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Api
{
	public sealed class ServerApi : RequestorCollection
	{
		public IStudentContoller StudentContoller { get; set; }
		public ITeacherController TeacherContoller { get; set; }
	}
}
