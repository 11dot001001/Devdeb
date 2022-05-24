namespace Devdeb.Serialization.Serializers.System.BigEndian
{
	public sealed class BigEndianUInt16Serializer : ConstantLengthSerializer<ushort>
	{
		static public BigEndianUInt16Serializer Default { get; } = new BigEndianUInt16Serializer();

		public BigEndianUInt16Serializer() : base(sizeof(ushort)) { }

		public unsafe override void Serialize(ushort instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			byte* instancePointer = (byte*)&instance;

			buffer[offset] = *(instancePointer + 1);
			buffer[offset + 1] = *instancePointer;
		}
		public unsafe override ushort Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			ushort instance;
			byte* instancePointer = (byte*)&instance;
			*(instancePointer + 1) = buffer[offset];
			*instancePointer = buffer[offset + 1];
			return instance;
		}
	}
}
