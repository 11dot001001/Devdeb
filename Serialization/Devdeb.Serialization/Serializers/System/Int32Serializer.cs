using Devdeb.Serialization.Serializers.System.BigEndian;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class Int32Serializer : ConstantLengthSerializer<int>
    {
        static public Int32Serializer Default { get; } = new Int32Serializer();
        static public BigEndianInt32Serializer BigEndian { get; } = BigEndianInt32Serializer.Default;

        public Int32Serializer() : base(sizeof(int)) { }

        public unsafe override void Serialize(int instance, byte[] buffer, int offset)
        {
            VerifySerialize(instance, buffer, offset);
            fixed (byte* bufferPointer = &buffer[offset])
                *(int*)bufferPointer = instance;
        }
        public unsafe override int Deserialize(byte[] buffer, int offset)
        {
            VerifyDeserialize(buffer, offset);
            fixed (byte* bufferPointer = &buffer[offset])
                return *(int*)bufferPointer;
        }
    }
}
