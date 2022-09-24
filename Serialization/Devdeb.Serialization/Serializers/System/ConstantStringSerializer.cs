using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class ConstantStringSerializer : IConstantLengthSerializer<string>
    {
        private readonly StringSerializer _serializer;
        private readonly int _bytesCount;

        public int Size => _bytesCount;

        public ConstantStringSerializer(StringSerializer stringSerializer, int bytesCount)
        {
            _serializer = stringSerializer ?? throw new ArgumentNullException(nameof(stringSerializer));
            _bytesCount = bytesCount;
        }

        public void Serialize(string instance, Span<byte> buffer)
        {
            int instanceSize = _serializer.GetSize(instance);
            if (instanceSize != _bytesCount)
                throw new ArgumentException($"Instance size not equal to {_bytesCount}.", nameof(instance));

            _serializer.Serialize(instance, buffer);
        }
        public string Deserialize(ReadOnlySpan<byte> buffer)
        {
            return _serializer.Deserialize(buffer[.._bytesCount]);
        }
    }
}
