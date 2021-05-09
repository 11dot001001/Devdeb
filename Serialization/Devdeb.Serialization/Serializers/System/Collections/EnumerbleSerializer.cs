using System;
using System.Collections.Generic;

namespace Devdeb.Serialization.Serializers.System.Collections
{
	public sealed class EnumerbleSerializer<T> : Serializer<IEnumerable<T>>
	{
		private readonly ISerializer<T> _elementSerializer;

		public EnumerbleSerializer(ISerializer<T> elementSerializer) : base(SerializerFlags.NeedCount)
		{
			_elementSerializer = elementSerializer ?? throw new ArgumentNullException(nameof(elementSerializer));
		}

		public override int Size(IEnumerable<T> instance)
		{
			VerifySize(instance);
			int size = 0;
			foreach(T element in instance)
                size = checked(size + _elementSerializer.Size(element));
			return size;
		}
		public override void Serialize(IEnumerable<T> instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			foreach(T element in instance)
				_elementSerializer.Serialize(element, buffer, ref offset);
		}
		public override IEnumerable<T> Deserialize(byte[] buffer, int offset, int? count = null)
		{
			VerifyDeserialize(buffer, offset, count);
			T[] instance = new T[count.Value];
			for (int i = 0; i != instance.Length; i++)
				instance[i] = _elementSerializer.Deserialize(buffer, ref offset, null);
			return instance;
		}
	}
}
