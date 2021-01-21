namespace Devdeb.Serialization.Serializers.System
{
	public class UInt64Serializer : ConstantLengthSerializer<ulong>
	{
		public UInt64Serializer() : base(sizeof(ulong)) { }

		public unsafe override void Serialize(ulong instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				*(ulong*)bufferPointer = instance;
		}
		public unsafe override ulong Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				return *(ulong*)bufferPointer;
		}
	}
}
