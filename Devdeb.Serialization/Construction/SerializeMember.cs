﻿using System;
using System.Reflection;

namespace Devdeb.Serialization.Construction
{
    internal sealed class SerializeMember
    {
		internal sealed class SerializerInfo
		{
			public SerializerInfo(object serializer)
			{
				Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
				SerializerType = serializer.GetType();
				SerializeMethodInfo = SerializerType.GetMethod(nameof(ISerializer<Type>.Serialize));
				DeserializeMethodInfo = SerializerType.GetMethod(nameof(ISerializer<Type>.Deserialize));
				GetBytesCountMethodInfo = SerializerType.GetMethod(nameof(ISerializer<Type>.GetBytesCount));
			}

			public object Serializer { get; }
			public Type SerializerType { get; }
			public MethodInfo SerializeMethodInfo { get; }
			public MethodInfo DeserializeMethodInfo { get; }
			public MethodInfo GetBytesCountMethodInfo { get; }
		}

        public MemberInfo Member;
        public SerializerInfo Serializer;

        public SerializeMember(MemberInfo member) : this(member, null) { }
        public SerializeMember(MemberInfo member, object serializer)
        {
            Member = member ?? throw new ArgumentNullException(nameof(member));
            if (serializer != null)
                Serializer = new SerializerInfo(serializer);
        }
    }
}