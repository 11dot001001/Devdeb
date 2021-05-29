using Devdeb.Serialization.Serializers.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Devdeb.Network.TCP.Rpc.Communication
{
	public class CommunicationMethodsMetaBuilder<TCommunicationInterface>
	{
		static private readonly Type _communicationInterface;

		static CommunicationMethodsMetaBuilder() => _communicationInterface = typeof(TCommunicationInterface);

		private readonly List<MethodInfo> _methodsInformations;

		public CommunicationMethodsMetaBuilder() => _methodsInformations = new List<MethodInfo>();

		public CommunicationMethodsMetaBuilder<TCommunicationInterface> AddPublicInstanceMethods()
		{
			_methodsInformations.AddRange(
				_communicationInterface
					.GetMethods(BindingFlags.Public | BindingFlags.Instance)
					.OrderBy(x => x.Name)
					.ToArray()
			);

			return this;
		}

		public CommunicationMethodMeta[] Build()
		{
			CommunicationMethodMeta[] communicationMethodMetas = new CommunicationMethodMeta[_methodsInformations.Count];
			for (int i = 0; i < _methodsInformations.Count; i++)
			{
				MethodInfo methodInfo = _methodsInformations[i];

				Type[] argumentTypes = methodInfo.GetParameters().Select(x => x.ParameterType).ToArray();

				ObjectArraySerializer argumentSerializer = null;
				bool doesNeedArguments = false;
				if (argumentTypes.Length != 0)
				{
					argumentSerializer = new ObjectArraySerializer(argumentTypes.Select(x => new ObjectSerializer(x)).ToArray());
					doesNeedArguments = true;
				}

				bool isAwaitingResult = TryGetResultSerializer(
					methodInfo,
					out ObjectSerializer resultSerializer,
					out bool isAsyncAwaitingResult,
					out Type resultType
				);

				communicationMethodMetas[i] = new CommunicationMethodMeta
				{
					Id = i,
					MethodInfo = methodInfo,
					ArgumentSerializer = argumentSerializer,
					ResultSerializer = resultSerializer,
					DoesNeedArguments = doesNeedArguments,
					IsAwaitingResult = isAwaitingResult,
					IsAsyncAwaitingResult = isAsyncAwaitingResult,
					ResultType = resultType
				};
			}

			return communicationMethodMetas;
		}

		private bool TryGetResultSerializer(
			MethodInfo methodInfo,
			out ObjectSerializer serializer,
			out bool isAsyncAwaitingResult,
			out Type resultType
		)
		{
			serializer = null;
			resultType = null;
			isAsyncAwaitingResult = false;

			if (methodInfo.ReturnType == typeof(void))
				return false;

			if (!typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
			{
				serializer = new ObjectSerializer(resultType = methodInfo.ReturnType);
				return true;
			}

			if (methodInfo.ReturnType.GenericTypeArguments.Length == 0)
				throw new Exception($"Unprocessed return parameter type: {nameof(Task)}. Method: {methodInfo}.");


			resultType = methodInfo.ReturnType.GenericTypeArguments[0];
			serializer = new ObjectSerializer(resultType);
			isAsyncAwaitingResult = true;
			return true;
		}
	}
}
