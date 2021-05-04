using System;
using System.Reflection;

namespace Devdeb.Serialization.Extensions
{
    static internal class MemberInfoExtensions
    {
        static internal Type GetFieldOrPropertyType(this MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo fieldInfo)
                return fieldInfo.FieldType;
            if (memberInfo is PropertyInfo propertyInfo)
                return propertyInfo.PropertyType;

            throw new Exception($"The {memberInfo} is not field or property.");
        }
    }
}
