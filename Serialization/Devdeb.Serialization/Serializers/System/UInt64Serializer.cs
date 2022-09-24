using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class UInt64Serializer : IConstantLengthSerializer<ulong>
    {
        static public UInt64Serializer Default { get; } = new UInt64Serializer();

        public int Size => sizeof(ulong);

        public unsafe void Serialize(ulong instance, Span<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                *(ulong*)bufferPointer = instance;
        }
        public unsafe ulong Deserialize(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                return *(ulong*)bufferPointer;
        }
    }
}
