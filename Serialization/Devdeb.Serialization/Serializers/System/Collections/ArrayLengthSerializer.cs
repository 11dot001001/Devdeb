using System;
using System.Linq;

namespace Devdeb.Serialization.Serializers.System.Collections
{
    public class ArrayLengthSerializer<T> : ISerializer<T[]>
    {
        private readonly EnumerbleLengthSerializer<T> _serializer;

        public ArrayLengthSerializer(ISerializer<T> elementSerializer)
        {
            if (elementSerializer == null)
                throw new ArgumentNullException(nameof(elementSerializer));
            _serializer = new EnumerbleLengthSerializer<T>(elementSerializer);
        }

        public int GetSize(T[] instance) => _serializer.GetSize(instance);
        public void Serialize(T[] instance, Span<byte> buffer) => _serializer.Serialize(instance, buffer);
        public T[] Deserialize(ReadOnlySpan<byte> buffer) => _serializer.Deserialize(buffer).ToArray();
    }
}
