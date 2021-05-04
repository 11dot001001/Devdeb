namespace Devdeb.Serialization.Serializers.System
{
    public sealed class UInt32Serializer : ConstantLengthSerializer<uint>
    {
        static public UInt32Serializer Default { get; } = new UInt32Serializer();

        public UInt32Serializer() : base(sizeof(uint)) { }

        public unsafe override void Serialize(uint instance, byte[] buffer, int offset)
        {
            VerifySerialize(instance, buffer, offset);
            fixed (byte* bufferPointer = &buffer[offset])
                *(uint*)bufferPointer = instance;
        }
        public unsafe override uint Deserialize(byte[] buffer, int offset)
        {
            VerifyDeserialize(buffer, offset);
            fixed (byte* bufferPointer = &buffer[offset])
                return *(uint*)bufferPointer;
        }
    }
}
