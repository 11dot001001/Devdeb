using System;

namespace Devdeb.Serialization.Serializers.Objects
{
	public class ObjectArraySerializer : ISerializer<object[]>
	{
		private readonly ObjectSerializer[] _objectSerializers;

		public ObjectArraySerializer(params ObjectSerializer[] objectSerializers)
		{
			_objectSerializers = objectSerializers ?? throw new ArgumentNullException(nameof(objectSerializers));
		}

        public int GetSize(object[] instance)
		{
			int size = 0;
			for (int i = 0; i != instance.Length; i++)
				size = checked(size + _objectSerializers[i].GetSize(instance[i]));
			return size;
		}

        public void Serialize(object[] instance, Span<byte> buffer)
        {
			int offset = 0;
			for (int i = 0; i != instance.Length; i++)
			{
				_objectSerializers[i].Serialize(instance[i], buffer[offset..]);
				offset += _objectSerializers[i].GetSize(instance[i]);
			}
		}

        public object[] Deserialize(ReadOnlySpan<byte> buffer)
		{
			int offset = 0;
			object[] instance = new object[_objectSerializers.Length];
			for (int i = 0; i != instance.Length; i++)
			{
				instance[i] = _objectSerializers[i].Deserialize(buffer[offset..]);
				offset += _objectSerializers[i].GetSize(instance[i]);
			}
			return instance;
		}
    }
}
