namespace Devdeb.Serialization.Serializers.System
{
	public sealed class UInt16Serializer : ConstantLengthSerializer<ushort>
	{
		static UInt16Serializer() => Default = new UInt16Serializer();
		static public UInt16Serializer Default { get; }

		public UInt16Serializer() : base(sizeof(ushort)) { }

		public unsafe override void Serialize(ushort instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				*(ushort*)bufferPointer = instance;
		}
		public unsafe override ushort Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				return *(ushort*)bufferPointer;
		}
	}
}
