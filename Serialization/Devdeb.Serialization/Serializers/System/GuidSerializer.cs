using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class GuidSerializer : IConstantLengthSerializer<Guid>
    {
        static public GuidSerializer Default { get; } = new GuidSerializer();

        public int Size => 16;

        public unsafe void Serialize(Guid instance, Span<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                *(Guid*)bufferPointer = instance;
        }
        public unsafe Guid Deserialize(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                return *(Guid*)bufferPointer;
        }
    }
}
