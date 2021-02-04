using Devdeb.Serialization.Builders.Info;
using Devdeb.Serialization.Default;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Devdeb.Serialization.Builders
{
	public class SerializerBuilder<T>
	{
		static internal bool MemberSelectionPredicate(MemberInfo memberInfo)
		{
			if (memberInfo == null)
				return false;
			if (memberInfo.DeclaringType != typeof(T))
				return false;
			if (memberInfo is FieldInfo fieldInfo)
			{
				if (!fieldInfo.IsPublic)
					return false;
			}
			else if (memberInfo is PropertyInfo propertyInfo)
			{
				if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
					return false;
				if (!propertyInfo.GetMethod.IsPublic || !propertyInfo.SetMethod.IsPublic)
					return false;
			}
			else
				return false;
			return true;
		}

		private readonly Type _serializationType;
		private List<MemberSerializaionInfo> _memberSerializaionInfos;
		private SerializerFlags _serializerFlags;

		public SerializerBuilder()
		{
			_serializationType = typeof(T);
			VerifyAccessModifier(_serializationType);
			_memberSerializaionInfos = new List<MemberSerializaionInfo>();
			_serializerFlags = SerializerFlags.Empty;
		}

		public ISerializer<T> Build()
		{
			if (_memberSerializaionInfos.Count == 0)
				throw new Exception("No serialize members specified.");
			return SerializerBuilder.Build<T>(new TypeSerializationInfo
			(
				_serializationType,
				_memberSerializaionInfos.ToArray(),
				_serializerFlags
			));
		}

		public SerializerBuilder<T> AddMember<TMember>(Expression<Func<T, TMember>> expression, ISerializer<TMember> memberSerializer)
		{
			VerifyMemberExpression(expression);
			MemberExpression memberExpression = expression.Body as MemberExpression;
			_memberSerializaionInfos.Add(new MemberSerializaionInfo(memberExpression.Member, memberSerializer));
			return this;
		}
		public SerializerBuilder<T> AddMember<TMember>(Expression<Func<T, TMember>> expression) => AddMember(expression, DefaultSerializer<TMember>.Instance);
		internal void AddMember(MemberInfo memberInfo)
		{
			VerifyMemberInfo(memberInfo);

			Type memberType = GetFieldOrPropertyType(memberInfo);
			Type defaultSerializer = typeof(DefaultSerializer<>).MakeGenericType(new[] { memberType });
			PropertyInfo instanceProperty = defaultSerializer.GetProperty(nameof(DefaultSerializer<object>.Instance));
			object serializer = instanceProperty.GetMethod.Invoke(null, null);
			if (serializer == null)
				throw new Exception($"Serializer for {memberType.Name} not found.");
			_memberSerializaionInfos.Add(new MemberSerializaionInfo(memberInfo, serializer));
		}

		private void VerifyMemberExpression<TMember>(Expression<Func<T, TMember>> expression)
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));
			if (!(expression.Body is MemberExpression memberExpression))
				throw new Exception("The specified member must be field or property.");
			VerifyMemberInfo(memberExpression.Member);
		}
		private void VerifyMemberInfo(MemberInfo memberInfo)
		{
			if (memberInfo == null)
				throw new ArgumentNullException(nameof(memberInfo));
			if (memberInfo.DeclaringType != _serializationType)
				throw new Exception($"The specified member must belong to declaring type: {_serializationType}.");
			if (memberInfo is FieldInfo fieldInfo)
			{
				if (!fieldInfo.IsPublic)
					throw new Exception($"The field {fieldInfo.FieldType.Name} is not public.");
			}
			else if (memberInfo is PropertyInfo propertyInfo)
			{
				if (!propertyInfo.CanRead)
					throw new Exception($"The property {propertyInfo.PropertyType.Name} is not readable.");
				if (!propertyInfo.GetMethod.IsPublic)
					throw new Exception($"The property {propertyInfo.PropertyType.Name} doesn't have public get method.");
				if (!propertyInfo.CanWrite)
					throw new Exception($"The property {propertyInfo.PropertyType.Name} is not writable.");
				if (!propertyInfo.SetMethod.IsPublic)
					throw new Exception($"The property {propertyInfo.PropertyType.Name} doesn't have public set method.");
			}
			else
				throw new Exception($"The {memberInfo} is not field or property.");
		}
		private void VerifyAccessModifier(Type type)
		{
			if (type.IsPublic)
				return;
			if (type.IsNestedPublic)
			{
				VerifyAccessModifier(type.DeclaringType);
				return;
			}
			throw new Exception($"The serialization type {_serializationType.FullName} must be in hierarchy public or nested public types.");
		}
		
		private Type GetFieldOrPropertyType(MemberInfo memberInfo)
		{
			if (memberInfo is FieldInfo fieldInfo)
				return fieldInfo.FieldType;
			else if (memberInfo is PropertyInfo propertyInfo)
				return propertyInfo.PropertyType;
			else
				throw new Exception($"The {memberInfo} is not field or property.");
		}
	}
}
