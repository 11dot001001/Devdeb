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
		public int MethodId;
		/// <remarks>Uses for definition of relations between request and response contexts.</remarks>
		public int Code;

		public override string ToString()
		{
			return $"MethodId: {MethodId}. Code: {Code}. Type: {Type}.";
		}
	}
}
