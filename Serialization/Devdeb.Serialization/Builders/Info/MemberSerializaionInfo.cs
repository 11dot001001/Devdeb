using Devdeb.Serialization.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace Devdeb.Serialization.Builders.Info
{
    internal class MemberSerializaionInfo
    {
        public MemberSerializaionInfo(MemberInfo memberInfo, object serializer)
        {
            MemberInfo = memberInfo ?? throw new ArgumentNullException(nameof(memberInfo));
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            Type serializerType = serializer.GetType();
            Type memberType = memberInfo.GetFieldOrPropertyType();
            Type expectedInterfaceImplementationType = typeof(ISerializer<>).MakeGenericType(new[] { memberType });

            if (!serializerType.GetInterfaces().Contains(expectedInterfaceImplementationType))
                throw new ArgumentException($"The specified {nameof(serializer)} does not match {nameof(memberInfo)} serialization.");
        }

        public MemberInfo MemberInfo { get; }
        public object Serializer { get; }
    }
}
