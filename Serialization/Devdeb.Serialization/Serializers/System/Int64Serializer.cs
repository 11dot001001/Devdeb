namespace Devdeb.Serialization.Serializers.System
{
	public class Int64Serializer : ConstantLengthSerializer<long>
	{
		public Int64Serializer() : base(sizeof(long)) { }

		public unsafe override void Serialize(long instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				*(long*)bufferPointer = instance;
		}
		public unsafe override long Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				return *(long*)bufferPointer;
		}
	}
}
