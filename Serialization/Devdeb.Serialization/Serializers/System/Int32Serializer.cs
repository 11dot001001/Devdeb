namespace Devdeb.Serialization.Serializers.System
{
	public class Int32Serializer : ConstantLengthSerializer<int>
	{
		public Int32Serializer() : base(sizeof(int)) { }

		public unsafe override void Serialize(int instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				*(int*)bufferPointer = instance;
		}
		public unsafe override int Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				return *(int*)bufferPointer;
		}
	}
}
