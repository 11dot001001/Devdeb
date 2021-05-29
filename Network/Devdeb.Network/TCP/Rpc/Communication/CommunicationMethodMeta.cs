using Devdeb.Serialization.Serializers.Objects;
using System;
using System.Reflection;

namespace Devdeb.Network.TCP.Rpc.Communication
{
	public class CommunicationMethodMeta
	{
		public int Id { get; internal set; }
		public MethodInfo MethodInfo { get; internal set; }
		public ObjectArraySerializer ArgumentSerializer { get; internal set; }
		public ObjectSerializer ResultSerializer { get; internal set; }
		public bool DoesNeedArguments{ get; internal set; }
		public bool IsAwaitingResult { get; internal set; }
		public bool IsAsyncAwaitingResult { get; internal set; }
		public Type ResultType { get; internal set; }
	}
}
