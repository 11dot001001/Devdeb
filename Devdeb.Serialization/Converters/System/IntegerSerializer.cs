using System;

namespace Devdeb.Serialization.Converters.System
{
    public sealed class IntegerSerializer : ConstantSerializer<int>
    {
        public IntegerSerializer() : base(sizeof(int)) { }

        public unsafe override void Serialize(int instance, byte[] buffer, ref int index)
        {
            fixed (byte* indexAddress = &buffer[index])
                *(int*)indexAddress = instance;
            index += BytesCount;
        }
        public unsafe override int Deserialize(byte[] buffer, ref int index)
        {
            int value;
            fixed (byte* bufferAddress = &buffer[index])
                value = *(int*)bufferAddress;
            index += BytesCount;
            return value;
        }
    }
}