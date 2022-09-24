using System;
using System.Collections.Generic;
using System.Drawing;

namespace Devdeb.Serialization.Serializers.System.Collections
{
    public sealed class EnumerbleSerializer<T> : ISerializer<IEnumerable<T>>
    {
        private readonly ISerializer<T> _elementSerializer;

        public EnumerbleSerializer(ISerializer<T> elementSerializer)
        {
            _elementSerializer = elementSerializer ?? throw new ArgumentNullException(nameof(elementSerializer));
        }
        public int GetSize(IEnumerable<T> instances)
        {
            int size = 0;
            foreach (var instance in instances)
                size = checked(size + _elementSerializer.GetSize(instance));
            return size;
        }
        public void Serialize(IEnumerable<T> instances, Span<byte> buffer)
        {
            int offset = 0;
            foreach (var instance in instances)
            {
                _elementSerializer.Serialize(instance, buffer[offset..]);
                offset += _elementSerializer.GetSize(instance);
            }
        }
        public IEnumerable<T> Deserialize(ReadOnlySpan<byte> buffer)
        {
            int offset = 0;
            List<T> instances = new();
            for (; offset < buffer.Length; )
            {
                var instance = _elementSerializer.Deserialize(buffer[offset..]);
                instances.Add(instance);
                offset += _elementSerializer.GetSize(instance);
            }
            return instances;
        }
    }
}
