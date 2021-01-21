namespace Devdeb.Serialization.Serializers.System
{
	public class ByteSerializer : ConstantLengthSerializer<byte>
	{
		public ByteSerializer() : base(sizeof(byte)) { }

		public unsafe override void Serialize(byte instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			buffer[offset] = instance;
		}
		public unsafe override byte Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			return buffer[offset];
		}
	}
}
