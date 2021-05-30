namespace Devdeb.Network.TCP.Rpc.Communication
{
	public class CommunicationMeta
	{
		public enum PackageType : byte
		{
			Request,
			Response
		}

		public PackageType Type;
		public int ControllerId;
		public int MethodId;
		public int ContextId;

		public override string ToString()
		{
			return $"ControllerId: {ControllerId}. MethodId: {MethodId}. ContextId: {ContextId}. Type: {Type}.";
		}
	}
}
