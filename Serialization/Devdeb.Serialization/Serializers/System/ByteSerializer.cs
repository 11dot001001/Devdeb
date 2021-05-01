namespace Devdeb.Serialization.Serializers.System
{
    public sealed class ByteSerializer : ConstantLengthSerializer<byte>
    {
        static public ByteSerializer Default { get; } = new ByteSerializer();

        public ByteSerializer() : base(sizeof(byte)) { }

        public unsafe override void Serialize(byte instance, byte[] buffer, int offset)
        {
            VerifySerialize(instance, buffer, offset);
            buffer[offset] = instance;
        }
        public unsafe override byte Deserialize(byte[] buffer, int offset)
        {
            VerifyDeserialize(buffer, offset);
            return buffer[offset];
        }
    }
}
