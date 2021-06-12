using Contracts.Client.Controllers;
using Devdeb.Network.TCP.Rpc.Requestor;

namespace Contracts.Client
{
	public sealed class ClientApi : RequestorCollection
	{
		public IStudentController ClientController { get; set; }
	}
}
