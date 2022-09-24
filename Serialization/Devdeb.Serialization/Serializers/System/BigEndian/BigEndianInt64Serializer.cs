using System;

namespace Devdeb.Serialization.Serializers.System.BigEndian
{
    public sealed class BigEndianInt64Serializer : IConstantLengthSerializer<long>
    {
        static public BigEndianInt64Serializer Default { get; } = new BigEndianInt64Serializer();

        public int Size => sizeof(long);

        public unsafe void Serialize(long instance, Span<byte> buffer)
        {
            byte* instancePointer = (byte*)&instance;

            buffer[0] = *(instancePointer + 7);
            buffer[1] = *(instancePointer + 6);
            buffer[2] = *(instancePointer + 5);
            buffer[3] = *(instancePointer + 4);
            buffer[4] = *(instancePointer + 3);
            buffer[5] = *(instancePointer + 2);
            buffer[6] = *(instancePointer + 1);
            buffer[7] = *instancePointer;
        }
        public unsafe long Deserialize(ReadOnlySpan<byte> buffer)
        {
            uint instance;
            byte* instancePointer = (byte*)&instance;
            *(instancePointer + 7) = buffer[0];
            *(instancePointer + 6) = buffer[1];
            *(instancePointer + 5) = buffer[2];
            *(instancePointer + 4) = buffer[3];
            *(instancePointer + 3) = buffer[4];
            *(instancePointer + 2) = buffer[5];
            *(instancePointer + 1) = buffer[6];
            *instancePointer = buffer[7];
            return instance;
        }
    }
}
