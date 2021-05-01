namespace Devdeb.Serialization.Serializers.System
{
    public sealed class UInt64Serializer : ConstantLengthSerializer<ulong>
    {
        static public UInt64Serializer Default { get; } = new UInt64Serializer();

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
