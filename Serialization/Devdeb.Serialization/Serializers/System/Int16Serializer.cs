namespace Devdeb.Serialization.Serializers.System
{
	public class Int16Serializer : ConstantLengthSerializer<short>
	{
		public Int16Serializer() : base(sizeof(short)) { }

		public unsafe override void Serialize(short instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				*(short*)bufferPointer = instance;
		}
		public unsafe override short Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				return *(short*)bufferPointer;
		}
	}
}
