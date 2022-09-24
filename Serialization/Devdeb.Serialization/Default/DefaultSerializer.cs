using Devdeb.Serialization.Builders;
using Devdeb.Serialization.Extensions;
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

			#region Enum
			if(serializationType.IsEnum)
			{
                Type underlyingType = Enum.GetUnderlyingType(serializationType);
                object underlyingTypeSerializer = GetSerializer(underlyingType);

                Type enumSerializerType = typeof(EnumSerializer<,>).MakeGenericType(new[] { serializationType, underlyingType });
				_serializer = (ISerializer<T>)Activator.CreateInstance(enumSerializerType, new[] { underlyingTypeSerializer });
				return;
			}
			#endregion
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
				_serializer = GetGenericSerializer(serializationType.GetElementType(), typeof(ArrayLengthSerializer<>));
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

			MemberInfo[] members = serializationType.GetMembers(BindingFlags.Public | BindingFlags.Instance).OrderBy(x => x.Name).ToArray();
			SerializerBuilder<T> serializerBuilder = new SerializerBuilder<T>();
			foreach (MemberInfo memberInfo in members.Where(x => SerializerBuilder<T>.MemberSelectionPredicate(x)))
			{
				Type memberType = memberInfo.GetFieldOrPropertyType();
				Type defaultSerializerType = typeof(DefaultSerializer<>).MakeGenericType(new[] { memberType });
				PropertyInfo instanceProperty = defaultSerializerType.GetProperty(nameof(DefaultSerializer<object>.Instance));
				serializer = instanceProperty.GetMethod.Invoke(null, null);

				if (memberType.IsClass)
				{
                    Type nullableMemberSerializerType = typeof(NullableSerializer<>).MakeGenericType(new[] { memberType });
					serializer = Activator.CreateInstance(nullableMemberSerializerType, new[] { serializer });
				}

				serializerBuilder.AddMember(memberInfo, serializer);
			}
			DefaultSerializersStorage.AddSerializer(serializationType, _serializer = serializerBuilder.Build());
		}

		static public ISerializer<T> Instance => _serializer;

		static private object GetSerializer(Type serializationType)
		{
			if (!DefaultSerializersStorage.TryGetSerializer(serializationType, out object serializer))
			{
				Type serializerType = typeof(DefaultSerializer<>).MakeGenericType(new[] { serializationType });
				serializer = serializerType.GetProperty(nameof(DefaultSerializer<object>.Instance)).GetMethod.Invoke(null, null);
			}
			if (serializer == null)
				throw new Exception($"Default serializer for type {serializationType} wasn't found.");

			return serializer;
		}
		static private ISerializer<T> GetGenericSerializer(Type genericParameterType, Type genericSerializerType)
		{
			object elementSerializer = GetSerializer(genericParameterType);
			Type serializerType = genericSerializerType.MakeGenericType(new[] { genericParameterType });
			return (ISerializer<T>)Activator.CreateInstance(serializerType, new[] { elementSerializer });
		}
	}
}
