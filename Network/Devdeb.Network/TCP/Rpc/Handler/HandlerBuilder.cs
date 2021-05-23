using Devdeb.Serialization.Default;
using Devdeb.Serialization;
using System;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace Devdeb.Network.TCP.Rpc.Handler
{
	internal class HandlerBuilder<THandler>
	{
		private static readonly Type _handlerType;

		static HandlerBuilder()
		{
			_handlerType = typeof(THandler);
		}

		public ProcessRequest<THandler>[] Build()
		{
			MethodInfo[] handlingMethods = _handlerType.GetMethods(BindingFlags.Public | BindingFlags.Instance).OrderBy(x => x.Name).ToArray();
			ProcessRequest<THandler>[] handlers = new ProcessRequest<THandler>[handlingMethods.Length];

			for (int handlerMethodIndex = 0; handlerMethodIndex < handlingMethods.Length; handlerMethodIndex++)
			{
				ParameterExpression bufferParameter = Expression.Parameter(typeof(byte[]), "buffer");
				ParameterExpression offsetParameter = Expression.Parameter(typeof(int).MakeByRefType(), "offset");
				ParameterExpression handlerParameter = Expression.Parameter(_handlerType, "server");

				MethodInfo handlingMethod = handlingMethods[handlerMethodIndex];
				Type[] argumentTypes = handlingMethod.GetParameters().Select(x => x.ParameterType).ToArray();

				ParameterExpression[] serializers = new ParameterExpression[argumentTypes.Length];
				ParameterExpression[] deserializedArguments = new ParameterExpression[argumentTypes.Length];
				Expression[] assigningSerializers = new Expression[argumentTypes.Length];
				Expression[] assigningDeserializedArguments = new Expression[argumentTypes.Length];

				for (int argumentIndex = 0; argumentIndex < argumentTypes.Length; argumentIndex++)
				{
					Type argumentType = argumentTypes[argumentIndex];
					Type serializerType = typeof(ISerializer<>).MakeGenericType(argumentType);

					ParameterExpression serializer = Expression.Variable(serializerType);
					ParameterExpression deserializedArgument = Expression.Variable(argumentType);

					MethodInfo getSerializer = typeof(DefaultSerializer<>)
													.MakeGenericType(argumentType)
													.GetProperty(nameof(DefaultSerializer<object>.Instance))
													.GetGetMethod();

					MethodInfo deserialize = serializerType.GetMethod(
						nameof(ISerializer<object>.Deserialize),
						new[] { typeof(byte[]), typeof(int).MakeByRefType(), typeof(int?) }
					);

					MethodCallExpression deserializeExpression = Expression.Call(
						serializer,
						deserialize,
						new Expression[] { bufferParameter, offsetParameter, Expression.Default(typeof(int?)) }
					);

					serializers[argumentIndex] = serializer;
					deserializedArguments[argumentIndex] = deserializedArgument;
					assigningSerializers[argumentIndex] = Expression.Assign(serializer, Expression.Call(getSerializer));
					assigningDeserializedArguments[argumentIndex] = Expression.Assign(deserializedArgument, deserializeExpression);
				}

				BlockExpression blockExpression = Expression.Block(
					serializers.Concat(deserializedArguments),
					assigningSerializers.Concat(assigningDeserializedArguments).Append(Expression.Call(handlerParameter, handlingMethod, deserializedArguments))
				);

				handlers[handlerMethodIndex] = Expression.Lambda<ProcessRequest<THandler>>(blockExpression, bufferParameter, offsetParameter, handlerParameter).Compile();

				//TODO: When will handle result
				//Type result = methodInfo.ReturnType;
			}
			return handlers;
		}
	}
}
