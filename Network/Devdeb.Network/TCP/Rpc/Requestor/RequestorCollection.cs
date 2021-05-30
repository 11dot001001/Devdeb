using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Rpc.Communication;
using System;
using System.Linq;
using System.Reflection;

namespace Devdeb.Network.TCP.Rpc.Requestor
{
	public abstract class RequestorCollection
	{
		private IRequestor[] _requestors;

		internal void InitializeRequestors(TcpCommunication tcpCommunication)
		{
			Type inheritedType = GetType();
			PropertyInfo[] requestorProperties = inheritedType
													.GetProperties(BindingFlags.Public | BindingFlags.Instance)
													.OrderBy(x => x.PropertyType.FullName)
													.ToArray();

			_requestors = new IRequestor[requestorProperties.Length];
			for (int i = 0; i < requestorProperties.Length; i++)
			{
				PropertyInfo requestorProperty = requestorProperties[i];
				Type requestorType = requestorProperty.PropertyType;

				Type rpcRequestorType = typeof(Requestor<>).MakeGenericType(requestorType);

				MethodInfo createRpcRequestor = rpcRequestorType.GetMethod(
					nameof(Requestor<object>.Create),
					new[] { typeof(TcpCommunication), typeof(int) }
				);

				IRequestor requestor = (IRequestor)createRpcRequestor.Invoke(null, new object[] { tcpCommunication, i });
				_requestors[i] = requestor;
				_ = requestorProperty.GetSetMethod().Invoke(this, new[] { requestor });
			}
		}

		internal void HandleResponse(CommunicationMeta meta, byte[] buffer, int offset)
		{
			_requestors[meta.ControllerId].HandleResponse(meta, buffer, offset);
		}
	}
}
