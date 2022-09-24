using Devdeb.Serialization.Default;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Devdeb.Serialization.Serializers.Objects
{
    public class ObjectSerializer : ISerializer<object>
    {
        static private object GetDefaultSerializer(Type type)
        {
            return typeof(DefaultSerializer<>)
                    .MakeGenericType(type)
                    .GetProperty(nameof(DefaultSerializer<object>.Instance))
                    .GetGetMethod()
                    .Invoke(null, null);
        }

        private readonly object _serializer;
        private readonly Type _serializerType;
        private readonly Type _objectType;
        private readonly MethodInfo _sizeMethod;
        private readonly MethodInfo _serializeMethod;
        private readonly MethodInfo _deserializeMethod;

        public ObjectSerializer(Type objectType)
            : this(objectType, GetDefaultSerializer(objectType)) { }

        public ObjectSerializer(Type objectType, object objectSerializer)
        {
            _objectType = objectType;
            _serializer = objectSerializer;
            _serializerType = _serializer.GetType();

            Type serializerInterfaceType = typeof(ISerializer<>).MakeGenericType(_objectType);
            InterfaceMapping serializerInterfaceMapping = _serializerType.GetInterfaceMap(serializerInterfaceType);

            for (int i = 0; i < serializerInterfaceMapping.InterfaceMethods.Length; i++)
            {
                if (serializerInterfaceMapping.InterfaceMethods[i] == serializerInterfaceType.GetMethod(nameof(ISerializer<object>.GetSize)))
                {
                    _sizeMethod = serializerInterfaceMapping.TargetMethods[i];
                    continue;
                }
                if (serializerInterfaceMapping.InterfaceMethods[i] == serializerInterfaceType.GetMethod(nameof(ISerializer<object>.Serialize), new[] { _objectType, typeof(Span<byte>) }))
                {
                    _serializeMethod = serializerInterfaceMapping.TargetMethods[i];
                    continue;
                }
                if (serializerInterfaceMapping.InterfaceMethods[i] == serializerInterfaceType.GetMethod(nameof(ISerializer<object>.Deserialize), new[] { typeof(Span<byte>) }))
                {
                    _deserializeMethod = serializerInterfaceMapping.TargetMethods[i];
                    continue;
                }
            }
        }

        public int GetSize(object instance)
        {
            return (int)_sizeMethod.Invoke(_serializer, new[] { instance });
        }

        public unsafe void Serialize(object instance, Span<byte> buffer)
        {
            fixed (byte* bufferPtr = buffer)
            _serializeMethod.Invoke(_serializer, new object[] { instance, new IntPtr(bufferPtr) });
        }

        public unsafe object Deserialize(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* bufferPtr = buffer)
                return _deserializeMethod.Invoke(_serializer, new object[] { new IntPtr(bufferPtr) });
        }
    }
}
