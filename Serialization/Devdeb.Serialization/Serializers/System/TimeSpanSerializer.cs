using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class TimeSpanSerializer : IConstantLengthSerializer<TimeSpan>
    {
        static public TimeSpanSerializer Default { get; } = new TimeSpanSerializer();

        public int Size => 8;

        public unsafe void Serialize(TimeSpan instance, Span<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                *(TimeSpan*)bufferPointer = instance;
        }
        public unsafe TimeSpan Deserialize(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                return *(TimeSpan*)bufferPointer;
        }
    }
}
