namespace Devdeb.Serialization.Serializers.System
{
	public sealed class UInt32Serializer : ConstantLengthSerializer<uint>
	{
		static UInt32Serializer() => Default = new UInt32Serializer();
		static public UInt32Serializer Default { get; }

		public UInt32Serializer() : base(sizeof(uint)) { }

		public unsafe override void Serialize(uint instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				*(int*)bufferPointer = (int)instance;
		}
		public unsafe override uint Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				return *(uint*)bufferPointer;
		}
	}
}
