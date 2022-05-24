namespace Devdeb.Serialization.Serializers.System.BigEndian
{
	public sealed class BigEndianInt64Serializer : ConstantLengthSerializer<long>
	{
		static public BigEndianInt64Serializer Default { get; } = new BigEndianInt64Serializer();

		public BigEndianInt64Serializer() : base(sizeof(long)) { }

		public unsafe override void Serialize(long instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);

			byte* instancePointer = (byte*)&instance;

			buffer[offset] = *(instancePointer + 7);
			buffer[offset + 1] = *(instancePointer + 6);
			buffer[offset + 2] = *(instancePointer + 5);
			buffer[offset + 3] = *(instancePointer + 4);
			buffer[offset + 4] = *(instancePointer + 3);
			buffer[offset + 5] = *(instancePointer + 2);
			buffer[offset + 6] = *(instancePointer + 1);
			buffer[offset + 7] = *instancePointer;
		}
		public unsafe override long Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			uint instance;
			byte* instancePointer = (byte*)&instance;
			*(instancePointer + 7) = buffer[offset];
			*(instancePointer + 6) = buffer[offset + 1];
			*(instancePointer + 5) = buffer[offset + 2];
			*(instancePointer + 4) = buffer[offset + 3];
			*(instancePointer + 3) = buffer[offset + 4];
			*(instancePointer + 2) = buffer[offset + 5];
			*(instancePointer + 1) = buffer[offset + 6];
			*instancePointer = buffer[offset + 7];
			return instance;
		}
	}
}
