using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Expecting;
using Devdeb.Network.TCP.Rpc.Communication;
using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Serialization;
using Devdeb.Serialization.Default;
using System.Net;
using System.Threading.Tasks;

namespace Devdeb.Network.TCP.Rpc
{
	public sealed class RpcClient<THandler, TRequestor> : BaseExpectingTcpClient
	{
		private readonly ISerializer<CommunicationMeta> _metaSerializer;
		private RpcRequestor<TRequestor> _requestor;

		public RpcClient(IPAddress iPAddress, int port) : base(iPAddress, port)
		{
			_metaSerializer = DefaultSerializer<CommunicationMeta>.Instance;
		}

		protected override void NotifyStarted() => _requestor = RpcRequestor<TRequestor>.Create(TcpCommunication);

		protected override void ProcessCommunication(TcpCommunication tcpCommunication, int count)
		{
			byte[] buffer = new byte[count];
			tcpCommunication.Receive(buffer, 0, count);

			Task.Factory.StartNew(() =>
			{
				int offset = 0;
				CommunicationMeta meta = _metaSerializer.Deserialize(buffer, ref offset);

				if (meta.Type == CommunicationMeta.PackageType.Response)
					_requestor.HandleResponse(meta, buffer, offset);
			});
		}

		public TRequestor Requestor => (TRequestor)(object)_requestor;
	}
}
