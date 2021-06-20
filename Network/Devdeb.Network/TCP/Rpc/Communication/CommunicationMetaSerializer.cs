using Devdeb.Serialization;
using Devdeb.Serialization.Serializers.System;
using static Devdeb.Network.TCP.Rpc.Communication.CommunicationMeta;

namespace Devdeb.Network.TCP.Rpc.Communication
{
	public sealed class CommunicationMetaSerializer : ConstantLengthSerializer<CommunicationMeta>
	{
		static public CommunicationMetaSerializer Default { get; } = new CommunicationMetaSerializer();

		public CommunicationMetaSerializer() : base(EnumSerializer<PackageType, byte>.Default.Size + Int32Serializer.Default.Size * 3) { }

		public override void Serialize(CommunicationMeta instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);

			EnumSerializer<PackageType, byte>.Default.Serialize(instance.Type, buffer, ref offset);
			Int32Serializer.Default.Serialize(instance.ControllerId, buffer, ref offset);
			Int32Serializer.Default.Serialize(instance.MethodId, buffer, ref offset);
			Int32Serializer.Default.Serialize(instance.ContextId, buffer, ref offset);
		}
		public override CommunicationMeta Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);

			CommunicationMeta instance = new CommunicationMeta
			{
				Type = EnumSerializer<PackageType, byte>.Default.Deserialize(buffer, ref offset),
				ControllerId = Int32Serializer.Default.Deserialize(buffer, ref offset),
				MethodId = Int32Serializer.Default.Deserialize(buffer, ref offset),
				ContextId = Int32Serializer.Default.Deserialize(buffer, ref offset),
			};
			return instance;
		}
	}
}
