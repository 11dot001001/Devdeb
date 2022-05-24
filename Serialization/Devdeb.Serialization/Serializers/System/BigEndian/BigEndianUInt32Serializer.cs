namespace Devdeb.Serialization.Serializers.System.BigEndian
{
	public class BigEndianUInt32Serializer : ConstantLengthSerializer<uint>
	{
		static public BigEndianUInt32Serializer Default { get; } = new BigEndianUInt32Serializer();

		public BigEndianUInt32Serializer() : base(sizeof(uint)) { }

		public unsafe override void Serialize(uint instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);

			byte* instancePointer = (byte*)&instance;

			buffer[offset] = *(instancePointer + 3);
			buffer[offset + 1] = *(instancePointer + 2);
			buffer[offset + 2] = *(instancePointer + 1);
			buffer[offset + 3] = *instancePointer;
		}
		public unsafe override uint Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			uint instance;
			byte* instancePointer = (byte*)&instance;
			*(instancePointer + 3) = buffer[offset];
			*(instancePointer + 2) = buffer[offset + 1];
			*(instancePointer + 1) = buffer[offset + 2];
			*instancePointer = buffer[offset + 3];
			return instance;
		}
	}
}
