using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Client;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Api
{
	public sealed class ClientApi : RequestorCollection
	{
		public IClientController ClientController { get; set; }
	}
}
