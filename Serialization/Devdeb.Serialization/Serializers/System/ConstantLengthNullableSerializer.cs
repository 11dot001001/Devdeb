using System;

namespace Devdeb.Serialization.Serializers.System
{
	public class ConstantLengthNullableSerializer<T> : ConstantLengthSerializer<T?> where T : struct
	{
		private readonly IConstantLengthSerializer<T> _elementSerializer;
		private readonly BooleanSerializer _booleanSerializer;

		public ConstantLengthNullableSerializer(IConstantLengthSerializer<T> elementSerializer)
			: base(new BooleanSerializer().Size + elementSerializer.Size, SerializerFlags.NullInstance)
		{
			_elementSerializer = elementSerializer ?? throw new ArgumentNullException(nameof(elementSerializer));
			_booleanSerializer = new BooleanSerializer();
		}

		public unsafe override void Serialize(T? instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			_booleanSerializer.Serialize(instance.HasValue, buffer, ref offset);
			T instanceValue = instance.HasValue ? instance.Value : default;
			_elementSerializer.Serialize(instanceValue, buffer, offset);
		}
		public unsafe override T? Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			bool hasValue = _booleanSerializer.Deserialize(buffer, ref offset);
			return !hasValue ? null : (T?)_elementSerializer.Deserialize(buffer, offset);
		}
	}
}
