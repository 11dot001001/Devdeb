namespace Devdeb.Serialization.Converters.System
{
    public class UIntegerSerializer : ConstantSerializer<uint>
    {
        public UIntegerSerializer() : base(sizeof(uint)) { }

        public unsafe override void Serialize(uint instance, byte[] buffer, ref int index)
        {
            fixed (byte* indexAddress = &buffer[index])
                *(uint*)indexAddress = instance;
            index += BytesCount;
        }
        public unsafe override uint Deserialize(byte[] buffer, ref int index)
        {
            uint value;
            fixed (byte* bufferAddress = &buffer[index])
                value = *(uint*)bufferAddress;
            index += BytesCount;
            return value;
        }
    }
}