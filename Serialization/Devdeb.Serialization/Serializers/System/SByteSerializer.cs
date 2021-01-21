namespace Devdeb.Serialization.Serializers.System
{
	public class SByteSerializer : ConstantLengthSerializer<sbyte>
	{
		public SByteSerializer() : base(sizeof(sbyte)) { }

		public unsafe override void Serialize(sbyte instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			buffer[offset] = (byte)instance;
		}
		public unsafe override sbyte Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			return (sbyte)buffer[offset];
		}
	}
}
