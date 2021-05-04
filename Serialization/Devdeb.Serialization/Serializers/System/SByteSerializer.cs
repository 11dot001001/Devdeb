namespace Devdeb.Serialization.Serializers.System
{
    public sealed class SByteSerializer : ConstantLengthSerializer<sbyte>
    {
        static public SByteSerializer Default { get; } = new SByteSerializer();

        public SByteSerializer() : base(sizeof(sbyte)) { }

        public unsafe override void Serialize(sbyte instance, byte[] buffer, int offset)
        {
            VerifySerialize(instance, buffer, offset);
            buffer[offset] = (byte)instance;
        }
        public unsafe override sbyte Deserialize(byte[] buffer, int offset)
        {
            VerifyDeserialize(buffer, offset);
            return (sbyte)buffer[offset];
        }
    }
}
