using Devdeb.Serialization.Default;
using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class EnumSerializer<TEnum, TUnderlying> : IConstantLengthSerializer<TEnum>
        where TEnum : Enum
        where TUnderlying : struct
    {
        static public EnumSerializer<TEnum, TUnderlying> Default = new();

        private readonly IConstantLengthSerializer<TUnderlying> _underlyingSerializer;

        public int Size => _underlyingSerializer.Size;

        public EnumSerializer() : this((IConstantLengthSerializer<TUnderlying>)DefaultSerializer<TUnderlying>.Instance) { }
        public EnumSerializer(IConstantLengthSerializer<TUnderlying> underlyingSerializer)
        {
            Type enumUnderlyingType = Enum.GetUnderlyingType(typeof(TEnum));
            Type underlyingType = typeof(TUnderlying);
            if (enumUnderlyingType != underlyingType)
                throw new Exception($"Underlying type of {nameof(TEnum)}: {enumUnderlyingType} doesn't equal {nameof(TUnderlying)}: {underlyingType}.");

            _underlyingSerializer = underlyingSerializer;
        }

        public void Serialize(TEnum instance, Span<byte> buffer)
        {
            _underlyingSerializer.Serialize((TUnderlying)(object)instance, buffer);
        }
        public TEnum Deserialize(ReadOnlySpan<byte> buffer)
        {
            return (TEnum)(object)_underlyingSerializer.Deserialize(buffer);
        }
    }
}
