using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LeanCode.CQRS.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public class AuthorizeWhenAttribute : Attribute
    {
        private Type authorizer { get; }
        private object customData { get; }

        public AuthorizeWhenAttribute(Type Authorizer, object CustomData = null)
        {
            authorizer = Authorizer;
            customData = CustomData;
        }

        public static List<AuthorizerDefinition> GetCustomAuthorizers(Type type)
        {
            return type.GetTypeInfo()
                .GetCustomAttributes<AuthorizeWhenAttribute>()
                .Select(AuthorizerDefinition.Create)
                .ToList();
        }

        public static List<AuthorizerDefinition> GetAuthorizers(object obj) => GetCustomAuthorizers(obj.GetType());

        public class AuthorizerDefinition
        {
            public Type Authorizer { get; }
            public object CustomData { get; }

            private AuthorizerDefinition(AuthorizeWhenAttribute attr)
            {
                this.Authorizer = attr.authorizer;
                this.CustomData = attr.customData;
            }

            internal static AuthorizerDefinition Create(AuthorizeWhenAttribute attr) =>
                new AuthorizerDefinition(attr);
        }
    }
}
