using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LeanCode.CQRS.Security
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class AuthorizeWithCheckerAttribute : Attribute
    {
        public Type AuthorizationChecker { get; }

        public AuthorizeWithCheckerAttribute(Type AuthorizationChecker)
        {
            this.AuthorizationChecker = AuthorizationChecker;
        }

        public static List<Type> GetCustomAccessCheckers(Type type)
        {
            return type.GetTypeInfo()
                .GetCustomAttributes<AuthorizeWithCheckerAttribute>()
                .Select(attr => attr.AuthorizationChecker)
                .ToList();
        }

        public static List<Type> GetAuthorizationCheckers(object obj) => GetCustomAccessCheckers(obj.GetType());
    }
}
