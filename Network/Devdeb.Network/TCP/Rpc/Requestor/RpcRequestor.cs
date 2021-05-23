using Devdeb.Network.TCP.Communication;
using Devdeb.Serialization.Default;
using Devdeb.Serialization;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using Devdeb.Network.TCP.Rpc.RpcRequest;

namespace Devdeb.Network.TCP.Rpc.Requestor
{
	public class RpcRequestor<TRequestor> : DispatchProxy
	{
		private class RequestorMeta
		{
			public RpcRequestMeta RequestMeta;
			public CalculateSize CalculateArgumentsSize;
			public Serialize SerializeArguments;
		}
		private delegate int CalculateSize(object[] arguments);
		private delegate void Serialize(byte[] buffer, ref int offset, object[] arguments);
		
		static private readonly Type _requestorType;

		static RpcRequestor()
		{
			_requestorType = typeof(TRequestor);
		}

		static public TRequestor Create(TcpCommunication tcpCommunication)
		{
			TRequestor proxy = Create<TRequestor, RpcRequestor<TRequestor>>();

			((RpcRequestor<TRequestor>)(object)proxy).Initialize(tcpCommunication);

			return proxy;
		}

		private TcpCommunication _tcpCommunication;
		private Dictionary<MemberInfo, RequestorMeta> _meta;
		private ISerializer<RpcRequestMeta> _requestMetaSerializer;

		private void Initialize(TcpCommunication tcpCommunication)
		{
			_tcpCommunication = tcpCommunication ?? throw new ArgumentNullException(nameof(tcpCommunication));
			_meta = new Dictionary<MemberInfo, RequestorMeta>();
			_requestMetaSerializer = DefaultSerializer<RpcRequestMeta>.Instance;

			MethodInfo[] requestMethods = _requestorType.GetMethods(BindingFlags.Public | BindingFlags.Instance).OrderBy(x => x.Name).ToArray();

			for (int requestMethodIndex = 0; requestMethodIndex < requestMethods.Length; requestMethodIndex++)
			{
				MethodInfo requestMethod = requestMethods[requestMethodIndex];

				Type[] argumentTypes = requestMethod.GetParameters().Select(x => x.ParameterType).ToArray();

				ParameterExpression[] serializers = new ParameterExpression[argumentTypes.Length];
				Expression[] assigningSerializers = new Expression[argumentTypes.Length];
				ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

				#region CalculateSize

				ParameterExpression sizeVariable = Expression.Variable(typeof(int), "size");
				BinaryExpression[] addSizeExpressions = new BinaryExpression[argumentTypes.Length];

				for (int argumentIndex = 0; argumentIndex < argumentTypes.Length; argumentIndex++)
				{
					Type argumentType = argumentTypes[argumentIndex];
					Type serializerType = typeof(ISerializer<>).MakeGenericType(argumentType);

					ParameterExpression serializer = Expression.Variable(serializerType);

					MethodInfo getSerializer = typeof(DefaultSerializer<>)
													.MakeGenericType(argumentType)
													.GetProperty(nameof(DefaultSerializer<object>.Instance))
													.GetGetMethod();

					MethodInfo size = serializerType.GetMethod(nameof(ISerializer<object>.Size));


					IndexExpression argumentAccess = Expression.ArrayAccess(argumentsParameter, Expression.Constant(argumentIndex));

					MethodCallExpression sizeExpression = Expression.Call(
						serializer,
						size,
						Expression.Convert(argumentAccess, argumentType)
					);

					serializers[argumentIndex] = serializer;
					assigningSerializers[argumentIndex] = Expression.Assign(serializer, Expression.Call(getSerializer));

					addSizeExpressions[argumentIndex] = Expression.AddAssignChecked(sizeVariable, sizeExpression);
				}

				BlockExpression calculateSizeExpression = Expression.Block(
					serializers.Append(sizeVariable),
					assigningSerializers
					.Append(Expression.Assign(sizeVariable, Expression.Default(typeof(int))))
					.Concat(addSizeExpressions)
				);

				CalculateSize calculateSize = Expression.Lambda<CalculateSize>(calculateSizeExpression, argumentsParameter).Compile();

				#endregion

				#region Serialize

				ParameterExpression bufferParameter = Expression.Parameter(typeof(byte[]), "buffer");
				ParameterExpression offsetParameter = Expression.Parameter(typeof(int).MakeByRefType(), "offset");
				MethodCallExpression[] serializeExpressions = new MethodCallExpression[argumentTypes.Length];

				for (int argumentIndex = 0; argumentIndex < argumentTypes.Length; argumentIndex++)
				{
					Type argumentType = argumentTypes[argumentIndex];
					Type serializerType = typeof(ISerializer<>).MakeGenericType(argumentType);

					ParameterExpression serializer = Expression.Variable(serializerType);

					MethodInfo getSerializer = typeof(DefaultSerializer<>)
													.MakeGenericType(argumentType)
													.GetProperty(nameof(DefaultSerializer<object>.Instance))
													.GetGetMethod();

					MethodInfo serializeInfo = serializerType.GetMethod(
						nameof(ISerializer<object>.Serialize),
						new[] { argumentType, typeof(byte[]), typeof(int).MakeByRefType() }
					);


					IndexExpression argumentAccess = Expression.ArrayAccess(argumentsParameter, Expression.Constant(argumentIndex));

					MethodCallExpression serializeCallExpression = Expression.Call(
						serializer,
						serializeInfo,
						new Expression[] { Expression.Convert(argumentAccess, argumentType), bufferParameter, offsetParameter }
					);

					serializers[argumentIndex] = serializer;
					assigningSerializers[argumentIndex] = Expression.Assign(serializer, Expression.Call(getSerializer));

					serializeExpressions[argumentIndex] = serializeCallExpression;
				}

				BlockExpression serializeExpression = Expression.Block(
					serializers,
					assigningSerializers.Concat(serializeExpressions)
				);

				Serialize serialize = Expression.Lambda<Serialize>(serializeExpression, bufferParameter, offsetParameter, argumentsParameter).Compile();

				#endregion

				_meta.Add(requestMethod, new RequestorMeta
				{
					RequestMeta = new RpcRequestMeta
					{
						MethodIndex = requestMethodIndex
					},
					CalculateArgumentsSize = calculateSize,
					SerializeArguments = serialize
				});
			}
		}

		protected override object Invoke(MethodInfo targetMethod, object[] args)
		{
			RequestorMeta meta = _meta[targetMethod];

			byte[] buffer = new byte[_requestMetaSerializer.Size(meta.RequestMeta) + meta.CalculateArgumentsSize(args)];
			int offset = 0;

			_requestMetaSerializer.Serialize(meta.RequestMeta, buffer, ref offset);
			meta.SerializeArguments(buffer, ref offset, args);

			_tcpCommunication.SendWithSize(buffer, 0, buffer.Length);

			return default;
		}
	}
}
