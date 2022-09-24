using System;

namespace Devdeb.Serialization.Builders.Info
{
	internal class TypeSerializationInfo
	{
		public TypeSerializationInfo(Type serializationType, MemberSerializaionInfo[] memberSerializaionInfos)
		{
			SerializationType = serializationType ?? throw new ArgumentNullException(nameof(serializationType));
			MemberSerializaionInfos = memberSerializaionInfos ?? throw new ArgumentNullException(nameof(memberSerializaionInfos));
		}

		public Type SerializationType { get; }
		public MemberSerializaionInfo[] MemberSerializaionInfos { get; }
	}
}
