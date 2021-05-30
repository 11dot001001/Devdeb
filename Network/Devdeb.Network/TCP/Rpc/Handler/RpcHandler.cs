using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Rpc.Communication;
using Devdeb.Serialization;
using Devdeb.Serialization.Default;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Devdeb.Network.TCP.Rpc.Handler
{
	public class RpcHandler<THandler>
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

					ConvertResult = typeof(RpcHandler<THandler>)
									.GetMethod(nameof(Convert), BindingFlags.Static | BindingFlags.NonPublic)
									.MakeGenericMethod(CommunicationMethodMeta.ResultType);
				}
			}
		}

		private readonly THandler _handlerInstance;
		private readonly HandlerMethodMeta[] _meta;
		private readonly ISerializer<CommunicationMeta> _metaSerializer;

		public RpcHandler(THandler handlerInstance)
		{
			_handlerInstance = handlerInstance ?? throw new ArgumentNullException(nameof(handlerInstance));
			CommunicationMethodMeta[] methodsMeta = new CommunicationMethodsMetaBuilder<THandler>().AddPublicInstanceMethods().Build();
			_metaSerializer = DefaultSerializer<CommunicationMeta>.Instance;
			_meta = methodsMeta.Select(x => new HandlerMethodMeta(x)).ToArray();
		}

		public void HandleRequest(
			TcpCommunication tcpCommunication,
			CommunicationMeta meta,
			byte[] buffer,
			int offset
		)
		{
			HandlerMethodMeta handlerMethodMeta = _meta.First(x => x.CommunicationMethodMeta.Id == meta.MethodId);
			var methodMeta = handlerMethodMeta.CommunicationMethodMeta;

			object[] arguments = null;

			if (methodMeta.DoesNeedArguments)
				arguments = methodMeta.ArgumentSerializer.Deserialize(buffer, ref offset);
			object result = methodMeta.MethodInfo.Invoke(_handlerInstance, arguments);

			if (!methodMeta.IsAwaitingResult)
				return;

			if (methodMeta.IsAsyncAwaitingResult)
				result = ((Task<object>)handlerMethodMeta.ConvertResult.Invoke(null, new[] { result })).Result;

			meta.Type = CommunicationMeta.PackageType.Response;
			buffer = new byte[_metaSerializer.Size(meta) + methodMeta.ResultSerializer.Size(result)];
			offset = 0;
			_metaSerializer.Serialize(meta, buffer, ref offset);
			methodMeta.ResultSerializer.Serialize(result, buffer, ref offset);
			tcpCommunication.SendWithSize(buffer, 0, buffer.Length);
		}

		static private async Task<object> Convert<T>(Task<T> task) => await task;
	}
}
