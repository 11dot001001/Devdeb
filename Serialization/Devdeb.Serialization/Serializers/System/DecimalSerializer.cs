namespace Devdeb.Serialization.Serializers.System
{
    public sealed class DecimalSerializer : ConstantLengthSerializer<decimal>
    {
        static public DecimalSerializer Default { get; } = new DecimalSerializer();

        public DecimalSerializer() : base(sizeof(decimal)) { }

        public unsafe override void Serialize(decimal instance, byte[] buffer, int offset)
        {
            VerifySerialize(instance, buffer, offset);
            fixed (byte* bufferPointer = &buffer[offset])
                *(decimal*)bufferPointer = instance;
        }
        public unsafe override decimal Deserialize(byte[] buffer, int offset)
        {
            VerifyDeserialize(buffer, offset);
            fixed (byte* bufferPointer = &buffer[offset])
                return *(decimal*)bufferPointer;
        }
    }
}
