using Devdeb.Serialization.Builders;
using Devdeb.Serialization.Serializers.System;
using Devdeb.Serialization.Serializers.System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Devdeb.Serialization.Default
{
	static public class DefaultSerializer<T>
	{
		static private readonly ISerializer<T> _serializer;

		static DefaultSerializer()
		{
			Type serializationType = typeof(T);
			if (DefaultSerializersStorage.TryGetSerializer(serializationType, out object serializer))
			{
				_serializer = (ISerializer<T>)serializer;
				return;
			}

			#region Nullable<T>
			Type elementType = Nullable.GetUnderlyingType(serializationType);
			if (elementType != null && elementType.IsValueType)
			{
				_serializer = GetGenericSerializer(elementType, typeof(ConstantLengthNullableSerializer<>));
				return;
			}
			#endregion
			#region T[]
			if (serializationType.IsArray)
			{
				_serializer = GetGenericSerializer(serializationType.GetElementType(), typeof(ArraySerializer<>));
				return;
			}
			#endregion
			#region IEnumerable<T>
			if (serializationType.IsGenericType && serializationType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				_serializer = GetGenericSerializer(serializationType.GetGenericArguments()[0], typeof(EnumerbleSerializer<>));
				return;
			}
			#endregion

			MemberInfo[] members = serializationType.GetMembers(BindingFlags.Public | BindingFlags.Instance);
			SerializerBuilder<T> serializerBuilder = new SerializerBuilder<T>();
			foreach (MemberInfo memberInfo in members.Where(x => SerializerBuilder<T>.MemberSelectionPredicate(x)))
				serializerBuilder.AddMember(memberInfo);
			DefaultSerializersStorage.AddSerializer(serializationType, _serializer = serializerBuilder.Build());
		}

		static public ISerializer<T> Instance => _serializer;

		static private ISerializer<T> GetGenericSerializer(Type genericParameterType, Type genericSerializerType)
		{
			if (!DefaultSerializersStorage.TryGetSerializer(genericParameterType, out object elementSerializer))
			{
				Type elementSerializerType = typeof(DefaultSerializer<>).MakeGenericType(new[] { genericParameterType });
				elementSerializer = elementSerializerType.GetProperty(nameof(DefaultSerializer<object>.Instance)).GetMethod.Invoke(null, null);
			}
			if (elementSerializer == null)
				throw new Exception($"Default serializer for type {genericParameterType} wasn't found.");

			Type serializerType = genericSerializerType.MakeGenericType(new[] { genericParameterType });
			return (ISerializer<T>)Activator.CreateInstance(serializerType, new[] { elementSerializer });
		}
	}
}
