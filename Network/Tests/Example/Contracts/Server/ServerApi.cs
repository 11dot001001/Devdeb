using Contracts.Server.Controllers;
using Devdeb.Network.TCP.Rpc.Requestor;

namespace Contracts.Server
{
	public sealed class ServerApi : RequestorCollection
	{
		public IStudentContoller StudentContoller { get; set; }
		public ITeacherController TeacherContoller { get; set; }
	}
}
