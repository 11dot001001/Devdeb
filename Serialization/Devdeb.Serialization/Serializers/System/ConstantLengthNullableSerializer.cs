using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class ConstantLengthNullableSerializer<T> : IConstantLengthSerializer<T?> where T : struct
    {
        private static readonly BooleanSerializer _booleanSerializer = BooleanSerializer.Default;
        
        private readonly IConstantLengthSerializer<T> _elementSerializer;

        public int Size => _booleanSerializer.Size + _elementSerializer.Size;

        public ConstantLengthNullableSerializer(IConstantLengthSerializer<T> elementSerializer)
        {
            _elementSerializer = elementSerializer ?? throw new ArgumentNullException(nameof(elementSerializer));
        }

        public void Serialize(T? instance, Span<byte> buffer)
        {
            _booleanSerializer.Serialize(instance.HasValue, buffer);
            T instanceValue = instance ?? default;
            _elementSerializer.Serialize(instanceValue, buffer[_booleanSerializer.Size..]);
        }
        public T? Deserialize(ReadOnlySpan<byte> buffer)
        {
            bool hasValue = _booleanSerializer.Deserialize(buffer);
            return !hasValue ? default : (T?)_elementSerializer.Deserialize(buffer[_booleanSerializer.Size..]);
        }
    }
}
