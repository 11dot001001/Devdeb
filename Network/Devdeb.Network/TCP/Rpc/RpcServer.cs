using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Expecting;
using Devdeb.Network.TCP.Rpc.Communication;
using Devdeb.Serialization;
using Devdeb.Serialization.Default;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Devdeb.Network.TCP.Rpc
{
	public sealed class RpcServer<THandler> : BaseExpectingTcpServer
	{
		private class HandlerMethodMeta
		{
			public CommunicationMethodMeta CommunicationMethodMeta { get; }
			public MethodInfo ConvertResult { get; }

			public HandlerMethodMeta(CommunicationMethodMeta communicationMethodMeta)
			{
				CommunicationMethodMeta = communicationMethodMeta ?? throw new ArgumentNullException(nameof(communicationMethodMeta));

				if (communicationMethodMeta.IsAwaitingResult && communicationMethodMeta.IsAsyncAwaitingResult)
				{

					ConvertResult = typeof(RpcServer<THandler>)
									.GetMethod(nameof(Convert), BindingFlags.Static | BindingFlags.NonPublic)
									.MakeGenericMethod(CommunicationMethodMeta.ResultType);
				}
			}
		}

		private readonly THandler _handlerInstance;
		private readonly ISerializer<CommunicationMeta> _metaSerializer;
		private readonly HandlerMethodMeta[] _meta;

		public RpcServer(IPAddress iPAddress, int port, int backlog, THandler handler) : base(iPAddress, port, backlog)
		{
			_handlerInstance = handler ?? throw new ArgumentNullException(nameof(handler));
			_metaSerializer = DefaultSerializer<CommunicationMeta>.Instance;
			CommunicationMethodMeta[] methodsMeta = new CommunicationMethodsMetaBuilder<THandler>().AddPublicInstanceMethods().Build();
			_meta = methodsMeta.Select(x => new HandlerMethodMeta(x)).ToArray();
		}

		protected override void ProcessCommunication(TcpCommunication tcpCommunication, int count)
		{
			byte[] buffer = new byte[count];
			tcpCommunication.Receive(buffer, 0, count);

			Task.Factory.StartNew(() =>
			{
				int offset = 0;
				CommunicationMeta requestMeta = _metaSerializer.Deserialize(buffer, ref offset);

				HandlerMethodMeta handlerMethodMeta = _meta.First(x => x.CommunicationMethodMeta.Id == requestMeta.MethodId);
				var methodMeta = handlerMethodMeta.CommunicationMethodMeta;

				object[] arguments = null;

				if (methodMeta.DoesNeedArguments)
					arguments = methodMeta.ArgumentSerializer.Deserialize(buffer, ref offset);
				object result = methodMeta.MethodInfo.Invoke(_handlerInstance, arguments);

				if (!methodMeta.IsAwaitingResult)
					return;

				if (methodMeta.IsAsyncAwaitingResult)
					result = ((Task<object>)handlerMethodMeta.ConvertResult.Invoke(null, new[] { result })).Result;

				requestMeta.Type = CommunicationMeta.PackageType.Response;
				buffer = new byte[_metaSerializer.Size(requestMeta) + methodMeta.ResultSerializer.Size(result)];
				offset = 0;
				_metaSerializer.Serialize(requestMeta, buffer, ref offset);
				methodMeta.ResultSerializer.Serialize(result, buffer, ref offset);
				tcpCommunication.SendWithSize(buffer, 0, buffer.Length);
			});
		}

		static private async Task<object> Convert<T>(Task<T> task) => await task;
	}
}
