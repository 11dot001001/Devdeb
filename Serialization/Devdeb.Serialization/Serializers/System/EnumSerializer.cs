using Devdeb.Serialization.Default;
using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class EnumSerializer<TEnum, TUnderlying> : ConstantLengthSerializer<TEnum>
        where TEnum : Enum
        where TUnderlying : struct
    {
        static public EnumSerializer<TEnum, TUnderlying> Default = new EnumSerializer<TEnum, TUnderlying>();

        private readonly IConstantLengthSerializer<TUnderlying> _underlyingSerializer;

        public EnumSerializer() : this((IConstantLengthSerializer<TUnderlying>)DefaultSerializer<TUnderlying>.Instance) { }
        public EnumSerializer(IConstantLengthSerializer<TUnderlying> underlyingSerializer) : base(underlyingSerializer.Size)
        {
            Type enumUnderlyingType = Enum.GetUnderlyingType(typeof(TEnum));
            Type underlyingType = typeof(TUnderlying);
            if (enumUnderlyingType != underlyingType)
                throw new Exception($"Underlying type of {nameof(TEnum)}: {enumUnderlyingType} doesn't equal {nameof(TUnderlying)}: {underlyingType}.");

            _underlyingSerializer = underlyingSerializer;
        }

        public unsafe override void Serialize(TEnum instance, byte[] buffer, int offset)
        {
            VerifySerialize(instance, buffer, offset);
            _underlyingSerializer.Serialize((TUnderlying)(object)instance, buffer, offset);
        }

        public override TEnum Deserialize(byte[] buffer, int offset)
        {
            VerifyDeserialize(buffer, offset);
            return (TEnum)(object)_underlyingSerializer.Deserialize(buffer, offset);
        }
    }
}
