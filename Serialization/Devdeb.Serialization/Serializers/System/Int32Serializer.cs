using Devdeb.Serialization.Serializers.System.BigEndian;
using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class Int32Serializer : IConstantLengthSerializer<int>
    {
        static public Int32Serializer Default { get; } = new Int32Serializer();
        static public BigEndianInt32Serializer BigEndian { get; } = BigEndianInt32Serializer.Default;

        public int Size => sizeof(int);

        public unsafe void Serialize(int instance, Span<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                *(int*)bufferPointer = instance;
        }
        public unsafe int Deserialize(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                return *(int*)bufferPointer;
        }
    }
}
