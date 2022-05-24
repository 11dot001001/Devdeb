namespace Devdeb.Serialization.Serializers.System.BigEndian
{
	public class BigEndianInt32Serializer : ConstantLengthSerializer<int>
	{
		static public BigEndianInt32Serializer Default { get; } = new BigEndianInt32Serializer();

		public BigEndianInt32Serializer() : base(sizeof(int)) { }

		public unsafe override void Serialize(int instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);

			byte* instancePointer = (byte*)&instance;
			
			buffer[offset] = *(instancePointer + 3);
			buffer[offset + 1] = *(instancePointer + 2);
			buffer[offset + 2] = *(instancePointer + 1);
			buffer[offset + 3] = *instancePointer;
		}
		public unsafe override int Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			int instance;
			byte* instancePointer = (byte*)&instance;
			*(instancePointer + 3) = buffer[offset];
			*(instancePointer + 2) = buffer[offset + 1];
			*(instancePointer + 1) = buffer[offset + 2];
			*instancePointer = buffer[offset + 3];
			return instance;
		}
	}
}
