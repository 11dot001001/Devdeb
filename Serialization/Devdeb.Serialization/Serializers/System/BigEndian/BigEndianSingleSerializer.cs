namespace Devdeb.Serialization.Serializers.System.BigEndian
{
	public class BigEndianSingleSerializer : ConstantLengthSerializer<float>
	{
		static public BigEndianSingleSerializer Default { get; } = new BigEndianSingleSerializer();

		public BigEndianSingleSerializer() : base(sizeof(float)) { }

		public unsafe override void Serialize(float instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);

			byte* instancePointer = (byte*)&instance;

			buffer[offset] = *(instancePointer + 3);
			buffer[offset + 1] = *(instancePointer + 2);
			buffer[offset + 2] = *(instancePointer + 1);
			buffer[offset + 3] = *instancePointer;
		}
		public unsafe override float Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			float instance;
			byte* instancePointer = (byte*)&instance;
			*(instancePointer + 3) = buffer[offset];
			*(instancePointer + 2) = buffer[offset + 1];
			*(instancePointer + 1) = buffer[offset + 2];
			*instancePointer = buffer[offset + 3];
			return instance;
		}
	}
}
