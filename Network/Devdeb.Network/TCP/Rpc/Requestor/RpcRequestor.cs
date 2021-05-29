using Devdeb.Network.TCP.Communication;
using Devdeb.Serialization.Default;
using Devdeb.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Devdeb.Network.TCP.Rpc.Communication;

namespace Devdeb.Network.TCP.Rpc.Requestor
{
	public class RpcRequestor<TRequestor> : DispatchProxy
	{
		private struct ReceivedResponce
		{
			public byte[] Buffer;
			public int Offset;
		}
		private class RequestorMethodMeta
		{
			public int RequestCount { get; set; }
			public object RequestCountLocker { get; }
			public CommunicationMethodMeta CommunicationMethodMeta { get; }
			public MethodInfo ConvertResult { get; }

			public RequestorMethodMeta(CommunicationMethodMeta communicationMethodMeta)
			{
				RequestCount = 0;
				RequestCountLocker = new object();
				CommunicationMethodMeta = communicationMethodMeta ?? throw new ArgumentNullException(nameof(communicationMethodMeta));

				if (communicationMethodMeta.IsAwaitingResult && communicationMethodMeta.IsAsyncAwaitingResult)
				{

					ConvertResult = typeof(RpcRequestor<TRequestor>)
									.GetMethod(nameof(Convert), BindingFlags.Static | BindingFlags.NonPublic)
									.MakeGenericMethod(CommunicationMethodMeta.ResultType);
				}
			}
		}
		private class ResponceWaitingMeta
		{
			public int MethodId;
			public int Code;
			public TaskCompletionSource<ReceivedResponce> TaskCompletionSource;
		}

		static public RpcRequestor<TRequestor> Create(TcpCommunication tcpCommunication)
		{
			RpcRequestor<TRequestor> proxy = (RpcRequestor<TRequestor>)(object)Create<TRequestor, RpcRequestor<TRequestor>>();

			proxy.Initialize(tcpCommunication);

			return proxy;
		}

		private TcpCommunication _tcpCommunication;
		private RequestorMethodMeta[] _meta;
		private ISerializer<CommunicationMeta> _requestMetaSerializer;
		private HashSet<ResponceWaitingMeta> _responceWaitingMetas;

		private void Initialize(TcpCommunication tcpCommunication)
		{
			_tcpCommunication = tcpCommunication ?? throw new ArgumentNullException(nameof(tcpCommunication));
			_requestMetaSerializer = DefaultSerializer<CommunicationMeta>.Instance;
			_responceWaitingMetas = new HashSet<ResponceWaitingMeta>();

			CommunicationMethodMeta[] methodsMeta = new CommunicationMethodsMetaBuilder<TRequestor>().AddPublicInstanceMethods().Build();
			_meta = methodsMeta.Select(x => new RequestorMethodMeta(x)).ToArray();
		}

		internal void HandleResponse(CommunicationMeta meta, byte[] buffer, int offset)
		{
			ResponceWaitingMeta responceMeta;
			lock (_responceWaitingMetas)
			{
				responceMeta = _responceWaitingMetas.First(x =>
				x.MethodId == meta.MethodId &&
				x.Code == meta.Code
				);
				_responceWaitingMetas.Remove(responceMeta);
			}

			responceMeta.TaskCompletionSource.SetResult(new ReceivedResponce
			{
				Buffer = buffer,
				Offset = offset
			});
		}

		protected override object Invoke(MethodInfo targetMethod, object[] args)
		{
			RequestorMethodMeta requestorMethodMeta = _meta.First(x => x.CommunicationMethodMeta.MethodInfo == targetMethod);

			int requestCode;
			lock (requestorMethodMeta.RequestCountLocker)
			{
				requestCode = requestorMethodMeta.RequestCount;
				requestorMethodMeta.RequestCount++;
			}

			CommunicationMethodMeta methodMeta = requestorMethodMeta.CommunicationMethodMeta;
			CommunicationMeta requestMeta = new CommunicationMeta
			{
				Type = CommunicationMeta.PackageType.Request,
				MethodId = methodMeta.Id,
				Code = requestCode
			};

			int bufferLength = _requestMetaSerializer.Size(requestMeta);
			if (methodMeta.DoesNeedArguments)
				bufferLength += methodMeta.ArgumentSerializer.Size(args);

			byte[] buffer = new byte[bufferLength];
			int offset = 0;

			_requestMetaSerializer.Serialize(requestMeta, buffer, ref offset);
			if (methodMeta.DoesNeedArguments)
				methodMeta.ArgumentSerializer.Serialize(args, buffer, ref offset);

			if (!methodMeta.IsAwaitingResult)
			{
				_tcpCommunication.SendWithSize(buffer, 0, buffer.Length);
				return null;
			}

			TaskCompletionSource<ReceivedResponce> taskCompletionSource = new TaskCompletionSource<ReceivedResponce>();
			lock (_responceWaitingMetas)
				_responceWaitingMetas.Add(new ResponceWaitingMeta
				{
					Code = requestCode,
					MethodId = methodMeta.Id,
					TaskCompletionSource = taskCompletionSource
				});

			Task<object> getObjectResultTask = taskCompletionSource.Task.ContinueWith(x =>
			{
				return methodMeta.ResultSerializer.Deserialize(x.Result.Buffer, x.Result.Offset);
			});

			_tcpCommunication.SendWithSize(buffer, 0, buffer.Length);

			if (!methodMeta.IsAsyncAwaitingResult)
				return getObjectResultTask.GetAwaiter().GetResult();

			return requestorMethodMeta.ConvertResult.Invoke(null, new[] { getObjectResultTask });
		}

		static private async Task<T> Convert<T>(Task<object> task) => (T)await task;
	}
}
