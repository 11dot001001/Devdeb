using Devdeb.Serialization.Converters.System;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Devdeb.Serialization.Construction
{
	static internal class SerializerBuilder
	{
		private const string _customSerializersAssemblyName = "Devdeb.Serialization.Customs";

		static private readonly ModuleBuilder _moduleBuilder;
		static private readonly Dictionary<Type, object> _defaultConverters;

		static SerializerBuilder()
		{
			AppDomain appDomain = AppDomain.CurrentDomain;
			AssemblyName assemblyName = new AssemblyName(_customSerializersAssemblyName);
			_moduleBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run).DefineDynamicModule(assemblyName.Name);
			_defaultConverters = new Dictionary<Type, object>
			{
				{ typeof(int), new IntegerSerializer() },
				{ typeof(uint), new UIntegerSerializer() },
				{ typeof(short), new ShortSerializer() }
			};
		}

		static private ISerializer<T> CreateDefaultSerializer<T>() => throw new NotImplementedException();
		static private ISerializer<T> GetDefaultSerializer<T>() => throw new NotImplementedException();


		static internal ISerializer<T> Create<T>(List<SerializeMember> serializeMembers)
		{
			Type serializeType = typeof(T);
			Type baseSerializerType = typeof(Serializer<>).MakeGenericType(new Type[] { serializeType });

			TypeBuilder typeBuilder = _moduleBuilder.DefineType(new StringBuilder(serializeType.FullName).Append(Guid.NewGuid().ToString()).ToString(), TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, serializeType);

			MethodInfo serializeMethodInfo = baseSerializerType.GetMethod(nameof(ISerializer<T>.Serialize));
			MethodInfo deserializeMethodInfo = baseSerializerType.GetMethod(nameof(ISerializer<T>.Deserialize));
			MethodInfo getBytesCountMethodInfo = baseSerializerType.GetMethod(nameof(ISerializer<T>.GetBytesCount));



			foreach (SerializeMember serializeMember in serializeMembers)
			{
				Type memberType;
				if (serializeMember.Member is PropertyInfo propertyInfo)
				{
					if (!propertyInfo.CanRead)
						throw new Exception();
					if (!propertyInfo.CanWrite)
						throw new Exception();
					memberType = propertyInfo.PropertyType;
				}
				else if (serializeMember.Member is FieldInfo fieldInfo)
				{

				}
			}

			return (ISerializer<T>)Activator.CreateInstance(typeBuilder.CreateTypeInfo());
		}
	}
}