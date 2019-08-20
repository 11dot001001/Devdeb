using Devdeb.Serialization.Converters.System;
using System;
using System.Collections.Generic;
using System.Linq;
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

		static private object CreateDefaultSerializer(Type type)
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.Public);
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Public);

			List<SerializeMember> serializeMembers = new List<SerializeMember>(fields.Length + properties.Length);
			foreach (FieldInfo fieldInfo in fields)
				serializeMembers.Add(new SerializeMember(fieldInfo));
			foreach (PropertyInfo propertyInfo in properties)
				serializeMembers.Add(new SerializeMember(propertyInfo));

			return Create(type, serializeMembers);
		}
		static private object GetDefaultSerializer(Type type)
		{
			if (_defaultConverters.TryGetValue(type, out object value))
				return value;
			object serializer = CreateDefaultSerializer(type);
			_defaultConverters.Add(type, serializer);
			return serializer;
		}


		static internal ISerializer<T> Create<T>(List<SerializeMember> serializeMembers) => (ISerializer<T>)Create(typeof(T), serializeMembers);
		static internal object Create(Type serializeType, List<SerializeMember> serializeMembers)
		{
			Type baseSerializerType = typeof(Serializer<>).MakeGenericType(new Type[] { serializeType });

			TypeBuilder typeBuilder = _moduleBuilder.DefineType(new StringBuilder(serializeType.FullName).Append(Guid.NewGuid().ToString()).ToString(), TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, serializeType);

			MethodInfo serializeMethodInfo = baseSerializerType.GetMethod(nameof(ISerializer<Type>.Serialize));
			MethodInfo deserializeMethodInfo = baseSerializerType.GetMethod(nameof(ISerializer<Type>.Deserialize));
			MethodInfo getBytesCountMethodInfo = baseSerializerType.GetMethod(nameof(ISerializer<Type>.GetBytesCount));

			foreach (SerializeMember serializeMember in serializeMembers)
			{
				Type memberType = null;
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
					memberType = fieldInfo.FieldType;
				}

				if (serializeMember.Serializer == null)
					serializeMember.Serializer = new SerializeMember.SerializerInfo(GetDefaultSerializer(memberType));
			}
			FieldBuilder[] fieldBuilders = new FieldBuilder[serializeMembers.Count];
			for (int i = 0; i < serializeMembers.Count; i++)
				fieldBuilders[i] = typeBuilder.DefineField("serializer_" + i, serializeMembers[i].Serializer.SerializerType, FieldAttributes.Private | FieldAttributes.InitOnly);

			Type[] constructorParameters = serializeMembers.Select(x => x.Serializer.SerializerType).ToArray();
			ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, constructorParameters);
			ILGenerator iLGenerator = constructorBuilder.GetILGenerator();

			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Call, baseSerializerType.GetConstructor(Type.EmptyTypes));
			for (int i = 0; i < serializeMembers.Count; i++)
			{
				iLGenerator.Emit(OpCodes.Ldarg, i);
				iLGenerator.Emit(OpCodes.Dup);
			}



			return Activator.CreateInstance(typeBuilder.CreateTypeInfo());
		}
	}
}