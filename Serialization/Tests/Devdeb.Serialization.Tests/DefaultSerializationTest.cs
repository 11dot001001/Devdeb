using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Devdeb.Serialization.Tests
{
	internal class DefaultSerializationTest
	{
		public void Test()
		{
			TestClass testClass = new TestClass
			{
				IntValue = 15,
				StringValue = "Some string"
			};
			MemberSerialization<TestClass> memberSerialization = new MemberSerialization<TestClass>();
			//memberSerialization.Add(x => x.IntValue, Int32Serializer.Default);
			memberSerialization.Add(x => x.AInstance, new ASerializer());

			ISerializer<TestClass> serializer = DefaultSerializer<TestClass>.Instance;
			byte[] buffer = new byte[serializer.Size(testClass)];
			serializer.Serialize(testClass, buffer, 0);
			TestClass result = serializer.Deserialize(buffer, 0);
		}

		static public class DefaultSerializer<T>
		{
			static private readonly ISerializer<T> _serializer;

			static DefaultSerializer() => _serializer = SerializerBuiler.Build<T>();

			static public ISerializer<T> Instance => _serializer;
		}

		static public class SerializerBuiler
		{
			static private readonly Dictionary<Type, object> _defaultSerializers;

			static SerializerBuiler()
			{
				_defaultSerializers = new Dictionary<Type, object>
				{
					[typeof(bool)] = BooleanSerializer.Default,
					[typeof(byte)] = ByteSerializer.Default,
					[typeof(char)] = CharSerializer.Default,
					[typeof(DateTime)] = DateTimeSerializer.Default,
					[typeof(decimal)] = DecimalSerializer.Default,
					[typeof(double)] = DoubleSerializer.Default,
					[typeof(Guid)] = GuidSerializer.Default,
					[typeof(short)] = Int16Serializer.Default,
					[typeof(int)] = Int32Serializer.Default,
					[typeof(long)] = Int64Serializer.Default,
					[typeof(sbyte)] = SByteSerializer.Default,
					[typeof(float)] = SingleSerializer.Default,
					[typeof(string)] = StringLengthSerializer.Default,
					[typeof(TimeSpan)] = TimeSpanSerializer.Default,
					[typeof(ushort)] = Int16Serializer.Default,
					[typeof(uint)] = UInt32Serializer.Default,
					[typeof(ulong)] = UInt64Serializer.Default
				};
			}

			static public ISerializer<T> CreateSerializer<T>(MemberSerialization<T>[] memberSerializations)
			{
				Type serializationType = typeof(T);
				serializationType.GetMembers();
				return null;
			}

			static public ISerializer<T> Build<T>()
			{
				Type instanceType = typeof(T);
				return null;
			}
		}

		public class MemberSerialization<T>
		{
			private MemberInfo _memberInfo;
			private object _serializer;

			public void Add<TMember>(Expression<Func<T, TMember>> targetMember, ISerializer<TMember> memberSerializer)
			{
				Type declaringType = typeof(T);
				if(!(targetMember.Body is MemberExpression memberExpression))
					throw new Exception("The specified member must be field or property.");
				if (memberExpression.Member.DeclaringType != declaringType)
					throw new Exception($"The specified member must belong to declaring type: {declaringType}.");
				Debug.Assert(memberSerializer.Flags.HasFlag(SerializerFlags.NeedCount));

				_memberInfo = memberExpression.Member;
				_serializer = memberSerializer;
			}

		}


		public class TestClass
		{
			public int IntValue2;
			public int IntValue { get; set; }
			public string StringValue { get; set; }
			public A AInstance { get; }

			public void AddTest() { }

		}
		public class A
		{
			public int C { get; set; }	
		}
		public sealed class ASerializer : Serializer<A>
		{
			public override int Size(A instance) => throw new NotImplementedException();
			public override void Serialize(A instance, byte[] buffer, int offset) => throw new NotImplementedException();
			public override A Deserialize(byte[] buffer, int offset, int? count = null) => throw new NotImplementedException();
		}
	}
}
