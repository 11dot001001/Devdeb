using Devdeb.Serialization.Serializers.System.BigEndian;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class Int16Serializer : ConstantLengthSerializer<short>
    {
        static public Int16Serializer Default { get; } = new Int16Serializer();
        static public BigEndianInt16Serializer BigEndian { get; } = BigEndianInt16Serializer.Default;

        public Int16Serializer() : base(sizeof(short)) { }

        public unsafe override void Serialize(short instance, byte[] buffer, int offset)
        {
            VerifySerialize(instance, buffer, offset);
            fixed (byte* bufferPointer = &buffer[offset])
                *(short*)bufferPointer = instance;
        }
        public unsafe override short Deserialize(byte[] buffer, int offset)
        {
            VerifyDeserialize(buffer, offset);
            fixed (byte* bufferPointer = &buffer[offset])
                return *(short*)bufferPointer;
        }
    }
}
