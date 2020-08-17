namespace Devdeb.Serialization.Converters.System
{
    public class ShortSerializer : ConstantSerializer<short>
    {
        public ShortSerializer() : base(sizeof(short)) { }

        public unsafe override void Serialize(short instance, byte[] buffer, ref int index)
        {
            fixed (byte* indexAddress = &buffer[index])
                *(short*)indexAddress = instance;
            index += BytesCount;
        }
        public unsafe override short Deserialize(byte[] buffer, ref int index)
        {
            short value;
            fixed (byte* bufferAddress = &buffer[index])
                value = *(short*)bufferAddress;
            index += BytesCount;
            return value;
        }
    }
}