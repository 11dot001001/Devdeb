namespace Devdeb.Serialization.Serializers.System
{
    public sealed class DoubleSerializer : ConstantLengthSerializer<double>
    {
        static public DoubleSerializer Default { get; } = new DoubleSerializer();

        public DoubleSerializer() : base(sizeof(double)) { }

        public unsafe override void Serialize(double instance, byte[] buffer, int offset)
        {
            VerifySerialize(instance, buffer, offset);
            fixed (byte* bufferPointer = &buffer[offset])
                *(double*)bufferPointer = instance;
        }
        public unsafe override double Deserialize(byte[] buffer, int offset)
        {
            VerifyDeserialize(buffer, offset);
            fixed (byte* bufferPointer = &buffer[offset])
                return *(double*)bufferPointer;
        }
    }
}
