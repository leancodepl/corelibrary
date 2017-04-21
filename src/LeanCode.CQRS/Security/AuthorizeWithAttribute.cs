using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LeanCode.CQRS.Security
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class AuthorizeWithAttribute : Attribute
    {
        public Type Authorizer { get; }

        public AuthorizeWithAttribute(Type Authorizer)
        {
            this.Authorizer = Authorizer;
        }

        public static List<Type> GetCustomAuthorizers(Type type)
        {
            return type.GetTypeInfo()
                .GetCustomAttributes<AuthorizeWithAttribute>()
                .Select(attr => attr.Authorizer)
                .ToList();
        }

        public static List<Type> GetAuthorizers(object obj) => GetCustomAuthorizers(obj.GetType());
    }
}
