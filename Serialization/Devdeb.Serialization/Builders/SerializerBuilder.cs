using Devdeb.Serialization.Builders.Info;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Devdeb.Serialization.Builders
{
	static internal class SerializerBuilder
	{
		private const string _builtSerializersAssembllyName = "Devdeb.BuiltSerializers";

		static private readonly ModuleBuilder _moduleBuilder;

		static SerializerBuilder()
		{
			AssemblyName assemblyName = new(_builtSerializersAssembllyName);
			AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			_moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.FullName);
		}

		static public ISerializer<T> Build<T>(TypeSerializationInfo typeSerializationInfo)
		{
			if (typeof(T) != typeSerializationInfo.SerializationType)
				throw new Exception($"The {nameof(T)} and {nameof(typeSerializationInfo.SerializationType)} must be equal.");
			return (ISerializer<T>)Build(typeSerializationInfo);
		}
		static public object Build(TypeSerializationInfo typeSerializationInfo)
		{
			Type interfaceGenericType = typeof(ISerializer<>).MakeGenericType(new[] { typeSerializationInfo.SerializationType });

			TypeBuilder typeBuilder = DefineType(interfaceGenericType, typeSerializationInfo);

			FieldBuilder[] serializersFields = BuildSerializersFields(typeBuilder, typeSerializationInfo);
			BuildConstructor(typeBuilder, serializersFields, typeSerializationInfo);
			BuildMethodGetSize(typeBuilder, serializersFields, interfaceGenericType, typeSerializationInfo);
			BuildMethodSerialize(typeBuilder, serializersFields, interfaceGenericType, typeSerializationInfo);
			BuildMethodDeserialize(typeBuilder, serializersFields, interfaceGenericType, typeSerializationInfo);

			Type builtType = typeBuilder.CreateTypeInfo();
			return Activator.CreateInstance(builtType, typeSerializationInfo.MemberSerializaionInfos.Select(x => x.Serializer).ToArray());
		}

		static private TypeBuilder DefineType(Type interfaceGenericType, TypeSerializationInfo typeSerializationInfo)
		{
			string serializerName = GetSerializerName(typeSerializationInfo.SerializationType);
			TypeAttributes typeAttributes = TypeAttributes.Class |
											TypeAttributes.Public |
											TypeAttributes.Sealed |
											TypeAttributes.AnsiClass |
											TypeAttributes.AutoClass |
											TypeAttributes.BeforeFieldInit;

			return _moduleBuilder.DefineType(serializerName, typeAttributes, null, new Type[] { interfaceGenericType });
		}
		static private FieldBuilder[] BuildSerializersFields(TypeBuilder typeBuilder, TypeSerializationInfo typeSerializationInfo)
		{
			FieldBuilder[] fieldBuilders = new FieldBuilder[typeSerializationInfo.MemberSerializaionInfos.Length];
			for (int i = 0; i < fieldBuilders.Length; i++)
			{
				MemberSerializaionInfo memberSerializaionInfo = typeSerializationInfo.MemberSerializaionInfos[i];

				fieldBuilders[i] = typeBuilder.DefineField
				(
					$"_serializers{i}",
					memberSerializaionInfo.Serializer.GetType(),
					FieldAttributes.Private | FieldAttributes.InitOnly
				);
			}
			return fieldBuilders;
		}
		static private void BuildConstructor(TypeBuilder typeBuilder, FieldBuilder[] serializersFields, TypeSerializationInfo typeSerializationInfo)
		{
			ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor
			(
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
				CallingConventions.Standard,
				typeSerializationInfo.MemberSerializaionInfos.Select(x => x.Serializer.GetType()).ToArray()
			);

			ILGenerator iLGenerator = constructorBuilder.GetILGenerator();
			for (int i = 0; i != serializersFields.Length; i++)
			{
				Label notNullSerializer = iLGenerator.DefineLabel();

				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldarg, i + 1);
				iLGenerator.Emit(OpCodes.Dup);
				iLGenerator.Emit(OpCodes.Brtrue_S, notNullSerializer);
				iLGenerator.Emit(OpCodes.Ldstr, typeSerializationInfo.MemberSerializaionInfos[i].Serializer.GetType().Name);
				iLGenerator.Emit(OpCodes.Newobj, typeof(ArgumentNullException).GetConstructor(new[] { typeof(string) }));
				iLGenerator.Emit(OpCodes.Throw);

				iLGenerator.MarkLabel(notNullSerializer);
				iLGenerator.Emit(OpCodes.Stfld, serializersFields[i]);
			}

			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
			iLGenerator.Emit(OpCodes.Ret);
		}
		static private void BuildMethodGetSize(
			TypeBuilder typeBuilder, 
			FieldBuilder[] serializersFields,
			Type interfaceGenericType, 
			TypeSerializationInfo typeSerializationInfo
		)
		{
			string methodname = nameof(ISerializer<object>.GetSize);
			Type[] methodParameters = new[] { typeSerializationInfo.SerializationType };
			Type returnType = typeof(int);

			MethodBuilder methodBuilder = typeBuilder.DefineMethod
			(
				methodname,
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final,
				returnType,
				methodParameters
			);
			typeBuilder.DefineMethodOverride(methodBuilder, interfaceGenericType.GetMethod(methodname, methodParameters));

			ILGenerator iLGenerator = methodBuilder.GetILGenerator();
			_ = iLGenerator.DeclareLocal(returnType);

			iLGenerator.Emit(OpCodes.Ldc_I4_0);
			iLGenerator.Emit(OpCodes.Stloc_0);
			for (int i = 0; i != serializersFields.Length; i++)
			{
				Type serializerType = serializersFields[i].FieldType;
				Type[] implementedInterfaces = serializerType.GetInterfaces();
				Type constantLengthSerializerInterface = implementedInterfaces.FirstOrDefault(x => x.GetGenericTypeDefinition() == typeof(IConstantLengthSerializer<>));

				if (constantLengthSerializerInterface != null)
				{
					#region Invoke IConstantLengthSerializer.Size
					PropertyInfo sizeProperty = constantLengthSerializerInterface.GetProperty(nameof(IConstantLengthSerializer<object>.Size));
					iLGenerator.Emit(OpCodes.Ldarg_0);
					iLGenerator.Emit(OpCodes.Ldfld, serializersFields[i]);
					iLGenerator.Emit(OpCodes.Callvirt, sizeProperty.GetMethod);
					#endregion
				}
				else
				{
					#region Invoke ISerializer.GetSize(instance)
					Type serializerInterface = implementedInterfaces.First(x => x.GetGenericTypeDefinition() == typeof(ISerializer<>));
					iLGenerator.Emit(OpCodes.Ldarg_0);
					iLGenerator.Emit(OpCodes.Ldfld, serializersFields[i]);

					iLGenerator.Emit(OpCodes.Ldarg_1);
					MemberInfo serilizationMemberType = typeSerializationInfo.MemberSerializaionInfos[i].MemberInfo;
					if (serilizationMemberType is FieldInfo fieldInfo)
						iLGenerator.Emit(OpCodes.Ldfld, fieldInfo);
					else if (serilizationMemberType is PropertyInfo propertyInfo)
						iLGenerator.Emit(OpCodes.Call, propertyInfo.GetMethod);

					iLGenerator.Emit(OpCodes.Callvirt, serializerInterface.GetMethod(nameof(ISerializer<object>.GetSize)));
					#endregion
				}
				iLGenerator.Emit(OpCodes.Ldloc_0);
				iLGenerator.Emit(OpCodes.Add);
				iLGenerator.Emit(OpCodes.Stloc_0);
			}
			iLGenerator.Emit(OpCodes.Ldloc_0);
			iLGenerator.Emit(OpCodes.Ret);
		}
		static private void BuildMethodSerialize(
			TypeBuilder typeBuilder,
			FieldBuilder[] serializersFields, 
			Type interfaceGenericType, 
			TypeSerializationInfo typeSerializationInfo
		)
		{
			string methodName = nameof(ISerializer<object>.Serialize);
			Type[] methodParameters = new[] { typeSerializationInfo.SerializationType, typeof(Span<byte>) };

			MethodBuilder methodBuilder = typeBuilder.DefineMethod
			(
				methodName,
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final,
				typeof(void),
				methodParameters
			);
			typeBuilder.DefineMethodOverride(methodBuilder, interfaceGenericType.GetMethod(methodName, methodParameters));

			ILGenerator iLGenerator = methodBuilder.GetILGenerator();
			for (int i = 0; i != serializersFields.Length; i++)
			{
				Type serializerType = serializersFields[i].FieldType;
				Type[] implementedInterfaces = serializerType.GetInterfaces();
				Type serializerInterface = implementedInterfaces.First(x => x.GetGenericTypeDefinition() == typeof(ISerializer<>));

				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldfld, serializersFields[i]);

				iLGenerator.Emit(OpCodes.Ldarg_1);
				MemberInfo serilizationMemberType = typeSerializationInfo.MemberSerializaionInfos[i].MemberInfo;
				Type memberType = null;
				if (serilizationMemberType is FieldInfo fieldInfo)
				{
					memberType = fieldInfo.FieldType;
					iLGenerator.Emit(OpCodes.Ldfld, fieldInfo);
				}
				else if (serilizationMemberType is PropertyInfo propertyInfo)
				{
					memberType = propertyInfo.PropertyType;
					iLGenerator.Emit(OpCodes.Callvirt, propertyInfo.GetMethod);
				}

				iLGenerator.Emit(OpCodes.Ldarg_2);
				iLGenerator.Emit(OpCodes.Callvirt, serializerInterface.GetMethod
				(
					nameof(ISerializer<object>.Serialize),
					new[] { memberType, typeof(Span<byte>) }
				));
			}
			iLGenerator.Emit(OpCodes.Ret);
		}
		static private void BuildMethodDeserialize(TypeBuilder typeBuilder, FieldBuilder[] serializersFields, Type interfaceGenericType, TypeSerializationInfo typeSerializationInfo)
		{
			string methodName = nameof(ISerializer<object>.Deserialize);
			Type[] methodParameters = new[] { typeof(ReadOnlySpan<byte>) };

			MethodBuilder methodBuilder = typeBuilder.DefineMethod
			(
				methodName,
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final,
				typeSerializationInfo.SerializationType,
				methodParameters
			);
			typeBuilder.DefineMethodOverride(methodBuilder, interfaceGenericType.GetMethod(methodName, methodParameters));

			ILGenerator iLGenerator = methodBuilder.GetILGenerator();
			_ = iLGenerator.DeclareLocal(typeSerializationInfo.SerializationType);
			iLGenerator.Emit(OpCodes.Newobj, typeSerializationInfo.SerializationType.GetConstructor(Type.EmptyTypes));
			iLGenerator.Emit(OpCodes.Stloc_0);
			for (int i = 0; i < serializersFields.Length; i++)
			{
				Type serializerType = serializersFields[i].FieldType;
				Type[] implementedInterfaces = serializerType.GetInterfaces();

				iLGenerator.Emit(OpCodes.Ldloc_0);
				#region Invoke ISerializer.Deserialize()
				Type serializerInterface = implementedInterfaces.First(x => x.GetGenericTypeDefinition() == typeof(ISerializer<>));
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldfld, serializersFields[i]);
				iLGenerator.Emit(OpCodes.Ldarg_1);
				iLGenerator.Emit(OpCodes.Callvirt, serializerInterface.GetMethod
				(
					nameof(ISerializer<object>.Deserialize),
					new[] { typeof(ReadOnlySpan<byte>) }
				));
				#endregion

				MemberInfo serilizationMemberType = typeSerializationInfo.MemberSerializaionInfos[i].MemberInfo;
				if (serilizationMemberType is FieldInfo fieldInfo)
					iLGenerator.Emit(OpCodes.Stfld, fieldInfo);
				else if (serilizationMemberType is PropertyInfo propertyInfo)
					iLGenerator.Emit(OpCodes.Callvirt, propertyInfo.SetMethod);
			}
			iLGenerator.Emit(OpCodes.Ldloc_0);
			iLGenerator.Emit(OpCodes.Ret);
		}
		static private string GetSerializerName(Type serializationType)
		{
			StringBuilder stringBuilder = new(serializationType.Namespace);
			stringBuilder.Append('_');
			stringBuilder.Append(Guid.NewGuid().ToString());
			stringBuilder.Append('_');
			stringBuilder.Append(serializationType.Name);
			return stringBuilder.ToString();
		}

		static private Type SearchHierarchyGenericTypeDefinition(Type type, Type genericTypeDefinition)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
				return type;
			for (; type.BaseType != null;)
			{
				type = type.BaseType;
				if (type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
					return type;
			}
			return null;
		}
	}
}
