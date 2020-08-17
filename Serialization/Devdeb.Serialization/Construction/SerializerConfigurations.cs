using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Devdeb.Serialization.Construction
{
    public sealed class SerializerConfigurations<T>
    {
        private readonly List<SerializeMember> _membersInformation;

        internal SerializerConfigurations(object membersInformation) => _membersInformation = (List<SerializeMember>)membersInformation;

        public void AddMember<TMember>(Expression<Func<T, TMember>> member, ISerializer<TMember> serializer)
        {
            if (!(member.Body is MemberExpression memberExpression))
                throw new Exception();
            _membersInformation.Add(new SerializeMember(memberExpression.Member, serializer));
        }
        public void AddMember<TMember>(Expression<Func<T, TMember>> member)
        {
            if (!(member.Body is MemberExpression memberExpression))
                throw new Exception();
            _membersInformation.Add(new SerializeMember(memberExpression.Member));
        }
    }
}