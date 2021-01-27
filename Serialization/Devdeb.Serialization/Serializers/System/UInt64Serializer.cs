namespace Devdeb.Serialization.Serializers.System
{
	public sealed class UInt64Serializer : ConstantLengthSerializer<ulong>
	{
		static UInt64Serializer() => Default = new UInt64Serializer();
		static public UInt64Serializer Default { get; }

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
