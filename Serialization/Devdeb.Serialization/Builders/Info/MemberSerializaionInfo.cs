using System;
using System.Reflection;

namespace Devdeb.Serialization.Builders.Info
{
	internal class MemberSerializaionInfo
	{
		public MemberSerializaionInfo(MemberInfo memberInfo, object serializer)
		{
			MemberInfo = memberInfo ?? throw new ArgumentNullException(nameof(memberInfo));
			Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
		}

		public MemberInfo MemberInfo { get; }
		public object Serializer { get; }
	}
}
