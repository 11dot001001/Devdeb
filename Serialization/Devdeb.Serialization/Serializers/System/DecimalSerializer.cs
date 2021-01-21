namespace Devdeb.Serialization.Serializers.System
{
	public class DecimalSerializer : ConstantLengthSerializer<decimal>
	{
		public DecimalSerializer() : base(sizeof(decimal)) { }

		public unsafe override void Serialize(decimal instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				*(decimal*)bufferPointer = instance;
		}
		public unsafe override decimal Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				return *(decimal*)bufferPointer;
		}
	}
}
