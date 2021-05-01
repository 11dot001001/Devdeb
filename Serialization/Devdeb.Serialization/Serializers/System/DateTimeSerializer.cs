using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class DateTimeSerializer : ConstantLengthSerializer<DateTime>
    {
        static public DateTimeSerializer Default { get; } = new DateTimeSerializer();

        public DateTimeSerializer() : base(8) { }

        public unsafe override void Serialize(DateTime instance, byte[] buffer, int offset)
        {
            VerifySerialize(instance, buffer, offset);
            fixed (byte* bufferPointer = &buffer[offset])
                *(long*)bufferPointer = instance.ToBinary();
        }
        public unsafe override DateTime Deserialize(byte[] buffer, int offset)
        {
            VerifyDeserialize(buffer, offset);
            fixed (byte* bufferPointer = &buffer[offset])
                return DateTime.FromBinary(*(long*)bufferPointer);
        }
    }
}
