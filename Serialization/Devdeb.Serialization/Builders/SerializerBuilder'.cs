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
		private readonly Type _serializationType;
		private List<MemberSerializaionInfo> _memberSerializaionInfos;
		private SerializerFlags _serializerFlags;

		public SerializerBuilder()
		{
			_serializationType = typeof(T);
			//check type on default .ctor
			_memberSerializaionInfos = new List<MemberSerializaionInfo>();
			_serializerFlags = SerializerFlags.Empty;
		}

		public void AddMember<TMember>(Expression<Func<T, TMember>> expression, ISerializer<TMember> memberSerializer)
		{
			VerifyMemberExpression(expression);
			MemberExpression memberExpression = expression.Body as MemberExpression;
			_memberSerializaionInfos.Add(new MemberSerializaionInfo(memberExpression.Member, memberSerializer));
		}
		public void AddMember<TMember>(Expression<Func<T, TMember>> expression) => AddMember(expression, DefaultSerializer<TMember>.Instance);
		internal void AddMember(MemberInfo memberInfo)
		{
			if (memberInfo == null)
				throw new ArgumentNullException(nameof(memberInfo));
			if (memberInfo.DeclaringType != _serializationType)
				throw new Exception($"The specified member must belong to declaring type: {_serializationType}.");

			Type memberType;
			if (memberInfo is FieldInfo fieldInfo)
			{
				if (!fieldInfo.IsPublic)
					throw new Exception($"The field {fieldInfo.FieldType.Name} is not public.");
				memberType = fieldInfo.FieldType;
			}
			else if (memberInfo is PropertyInfo propertyInfo)
			{
				if (!propertyInfo.CanRead)
					throw new Exception($"The property {propertyInfo.PropertyType.Name} is not readable.");
				if (!propertyInfo.CanWrite)
					throw new Exception($"The property {propertyInfo.PropertyType.Name} is not writable.");
				memberType = propertyInfo.PropertyType;
			}
			else
				throw new Exception($"The {memberInfo.Name} is not field or property.");

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
			if (memberExpression.Member.DeclaringType != _serializationType)
				throw new Exception($"The specified member must belong to declaring type: {_serializationType}.");
			if (memberExpression.Member is PropertyInfo propertyInfo)
			{
				//check consists whether public get and set methods. Also public access modifier for fields.
			}
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
	}
}
