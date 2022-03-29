using Devdeb.Audio.InternetTelephony.Contracts.Client.Controllers;
using Devdeb.Network.TCP.Rpc.Requestor;

namespace Devdeb.Audio.InternetTelephony.Contracts.Client
{
	public sealed class ClientApi : RequestorCollection
	{
		public ICallController CallController { get; set; }
	}
}
