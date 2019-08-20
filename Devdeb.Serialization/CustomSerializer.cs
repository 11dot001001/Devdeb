using Devdeb.Serialization.Construction;
using System.Collections.Generic;

namespace Devdeb.Serialization
{
    public abstract class CustomSerializer<T> : Serializer<T>
    {
        static private readonly List<SerializeMember> _serializeMembers;
        static private ISerializer<T> _serializer;
        static private bool _isCreated;
        static private readonly object _creatingKey;

        static CustomSerializer()
        {
            _serializeMembers = new List<SerializeMember>();
            _creatingKey = new object();
        }

        public CustomSerializer()
        {
            lock (_creatingKey)
            {
                if (_isCreated)
                    return;

                Configure(new SerializerConfigurations<T>(_serializeMembers));
                _serializer = SerializerBuilder.Create<T>(_serializeMembers);
                _isCreated = true;
            }
        }

        public sealed override void Serialize(T instance, byte[] buffer, ref int index) => _serializer.Serialize(instance, buffer, ref index);
        public sealed override T Deserialize(byte[] buffer, ref int index) => _serializer.Deserialize(buffer, ref index);
        public sealed override int GetBytesCount(T instance) => _serializer.GetBytesCount(instance);

        protected abstract void Configure(SerializerConfigurations<T> serializerSettings);
    }
}