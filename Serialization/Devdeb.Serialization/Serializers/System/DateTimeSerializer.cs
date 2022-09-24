using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class DateTimeSerializer : IConstantLengthSerializer<DateTime>
    {
        static public DateTimeSerializer Default { get; } = new DateTimeSerializer();

        public int Size => 8;

        public unsafe void Serialize(DateTime instance, Span<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                *(long*)bufferPointer = instance.ToBinary();
        }
        public unsafe DateTime Deserialize(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                return DateTime.FromBinary(*(long*)bufferPointer);
        }
    }
}
