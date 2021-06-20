using Devdeb.DependencyInjection.Extensions;
using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Rpc.Communication;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IServiceProvider = Devdeb.DependencyInjection.IServiceProvider;

namespace Devdeb.Network.TCP.Rpc.Handler
{
	public class ControllerHandler<THandler> : IControllerHandler
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

					ConvertResult = typeof(ControllerHandler<THandler>)
									.GetMethod(nameof(Convert), BindingFlags.Static | BindingFlags.NonPublic)
									.MakeGenericMethod(CommunicationMethodMeta.ResultType);
				}
			}
		}

		private readonly HandlerMethodMeta[] _meta;

		public ControllerHandler()
		{
			CommunicationMethodMeta[] methodsMeta = new CommunicationMethodsMetaBuilder<THandler>().AddPublicInstanceMethods().Build();
			_meta = methodsMeta.Select(x => new HandlerMethodMeta(x)).ToArray();
		}

		public void HandleRequest(
			IServiceProvider serviceProvider,
			TcpCommunication tcpCommunication,
			CommunicationMeta meta,
			byte[] buffer,
			int offset
		)
		{
			THandler controller = serviceProvider.GetRequiredService<THandler>();

			HandlerMethodMeta handlerMethodMeta = _meta.First(x => x.CommunicationMethodMeta.Id == meta.MethodId);
			var methodMeta = handlerMethodMeta.CommunicationMethodMeta;

			object[] arguments = null;

			if (methodMeta.DoesNeedArguments)
				arguments = methodMeta.ArgumentSerializer.Deserialize(buffer, ref offset);
			object result = methodMeta.MethodInfo.Invoke(controller, arguments);

			if (!methodMeta.IsAwaitingResult)
				return;

			if (methodMeta.IsAsyncAwaitingResult)
				result = ((Task<object>)handlerMethodMeta.ConvertResult.Invoke(null, new[] { result })).Result;

			meta.Type = CommunicationMeta.PackageType.Response;
			buffer = new byte[CommunicationMetaSerializer.Default.Size + methodMeta.ResultSerializer.Size(result)];
			offset = 0;
			CommunicationMetaSerializer.Default.Serialize(meta, buffer, ref offset);
			methodMeta.ResultSerializer.Serialize(result, buffer, ref offset);
			tcpCommunication.SendWithSize(buffer, 0, buffer.Length);
		}

		static private async Task<object> Convert<T>(Task<T> task) => await task;
	}
}
