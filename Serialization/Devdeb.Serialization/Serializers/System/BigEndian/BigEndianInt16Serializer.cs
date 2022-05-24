namespace Devdeb.Serialization.Serializers.System.BigEndian
{
	public sealed class BigEndianInt16Serializer : ConstantLengthSerializer<short>
	{
		static public BigEndianInt16Serializer Default { get; } = new BigEndianInt16Serializer();

		public BigEndianInt16Serializer() : base(sizeof(short)) { }

		public unsafe override void Serialize(short instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			byte* instancePointer = (byte*)&instance;

			buffer[offset] = *(instancePointer + 1);
			buffer[offset + 1] = *instancePointer;
		}
		public unsafe override short Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			short instance;
			byte* instancePointer = (byte*)&instance;
			*(instancePointer + 1) = buffer[offset];
			*instancePointer = buffer[offset + 1];
			return instance;
		}
	}
}
