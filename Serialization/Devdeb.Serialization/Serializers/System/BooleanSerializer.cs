namespace Devdeb.Serialization.Serializers.System
{
    public sealed class BooleanSerializer : ConstantLengthSerializer<bool>
    {
        static public BooleanSerializer Default { get; } = new BooleanSerializer();

        public BooleanSerializer() : base(sizeof(bool)) { }

        public unsafe override void Serialize(bool instance, byte[] buffer, int offset)
        {
            VerifySerialize(instance, buffer, offset);
            fixed (byte* bufferPointer = &buffer[offset])
                *(bool*)bufferPointer = instance;
        }
        public unsafe override bool Deserialize(byte[] buffer, int offset)
        {
            VerifyDeserialize(buffer, offset);
            fixed (byte* bufferPointer = &buffer[offset])
                return *(bool*)bufferPointer;
        }
    }
}
