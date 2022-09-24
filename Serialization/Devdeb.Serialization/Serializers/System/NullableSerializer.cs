using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class NullableSerializer<T> : ISerializer<T>
    {
        private readonly ISerializer<T> _elementSerializer;
        private readonly BooleanSerializer _booleanSerializer;

        public NullableSerializer(ISerializer<T> elementSerializer)
        {
            _elementSerializer = elementSerializer ?? throw new ArgumentNullException(nameof(elementSerializer));
            _booleanSerializer = new BooleanSerializer();
        }

        public int GetSize(T instance)
        {
            int size = _booleanSerializer.Size;
            if (instance != null)
                size += _elementSerializer.GetSize(instance);
            return size;
        }

        public void Serialize(T instance, Span<byte> buffer)
        {
            bool hasValue = instance != null;
            _booleanSerializer.Serialize(hasValue, buffer);
            if (!hasValue)
                return;
            _elementSerializer.Serialize(instance, buffer[_booleanSerializer.Size..]);
        }
        public T Deserialize(ReadOnlySpan<byte> buffer)
        {
            if (!_booleanSerializer.Deserialize(buffer))
                return default;
            return _elementSerializer.Deserialize(buffer[_booleanSerializer.Size..]);
        }
    }
}
