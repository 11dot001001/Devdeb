using Devdeb.Serialization.Extensions;
using System;
using System.Collections.Generic;

namespace Devdeb.Serialization.Serializers.System.Collections
{
    public class EnumerbleLengthSerializer<T> : ISerializer<IEnumerable<T>>
    {
        private readonly Int32Serializer _int32Serializer;
        private readonly ISerializer<T> _elementSerializer;
        private readonly EnumerbleSerializer<T> _enumerableSerializer;

        public EnumerbleLengthSerializer(ISerializer<T> elementSerializer)
        {
            _int32Serializer = Int32Serializer.Default;
            _elementSerializer = elementSerializer ?? throw new ArgumentNullException(nameof(elementSerializer));
            _enumerableSerializer = new EnumerbleSerializer<T>(elementSerializer);
        }

        public int GetSize(IEnumerable<T> instances)
        {
            return checked(_int32Serializer.Size + _enumerableSerializer.GetSize(instances));
        }
        public void Serialize(IEnumerable<T> instances, Span<byte> buffer)
        {
            var countingEnumerable = new CountingEnumerable<T>(instances);
            _enumerableSerializer.Serialize(countingEnumerable, buffer[_int32Serializer.Size..]);
            _int32Serializer.Serialize(countingEnumerable.Count, buffer);
        }
        public IEnumerable<T> Deserialize(ReadOnlySpan<byte> buffer)
        {
            int instancesCount = _int32Serializer.Deserialize(buffer);

            T[] instances = new T[instancesCount];

            int offset = _int32Serializer.Size;
            for (int i = 0; i != instancesCount; i++)
            {
                instances[i] = _elementSerializer.Deserialize(buffer[offset..]);
                offset += _elementSerializer.GetSize(instances[i]);
            }

            return instances;
        }
    }
}
