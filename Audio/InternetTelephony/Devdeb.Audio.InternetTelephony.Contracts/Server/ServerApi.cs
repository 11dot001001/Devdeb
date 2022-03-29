using Devdeb.Audio.InternetTelephony.Contracts.Server.Controllers;
using Devdeb.Network.TCP.Rpc.Requestor;

namespace Devdeb.Audio.InternetTelephony.Contracts.Server
{
	public sealed class ServerApi : RequestorCollection
	{
		public IUserController UserController { get; set; }
		public ICallController CallController { get; set; }
	}
}
