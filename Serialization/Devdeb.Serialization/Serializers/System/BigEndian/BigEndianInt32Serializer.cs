using System;

namespace Devdeb.Serialization.Serializers.System.BigEndian
{
    public class BigEndianInt32Serializer : IConstantLengthSerializer<int>
    {
        static public BigEndianInt32Serializer Default { get; } = new BigEndianInt32Serializer();

        public int Size => sizeof(int);

        public unsafe void Serialize(int instance, Span<byte> buffer)
        {
            byte* instancePointer = (byte*)&instance;

            buffer[0] = *(instancePointer + 3);
            buffer[1] = *(instancePointer + 2);
            buffer[2] = *(instancePointer + 1);
            buffer[3] = *instancePointer;
        }
        public unsafe int Deserialize(ReadOnlySpan<byte> buffer)
        {
            int instance;
            byte* instancePointer = (byte*)&instance;
            *(instancePointer + 3) = buffer[0];
            *(instancePointer + 2) = buffer[1];
            *(instancePointer + 1) = buffer[2];
            *instancePointer = buffer[3];
            return instance;
        }
    }
}
