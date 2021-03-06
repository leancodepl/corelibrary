using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LeanCode.CQRS.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public class AuthorizeWhenAttribute : Attribute
    {
        private readonly Type authorizerType;
        private readonly object? customData;

        public AuthorizeWhenAttribute(Type authorizerType, object? customData = null)
        {
            this.authorizerType = authorizerType;
            this.customData = customData;
        }

        public static List<AuthorizerDefinition> GetCustomAuthorizers(Type type)
        {
            return type
                .GetCustomAttributes<AuthorizeWhenAttribute>()
                .Select(AuthorizerDefinition.Create)
                .ToList();
        }

        public sealed class AuthorizerDefinition
        {
            public Type Authorizer { get; }
            public object? CustomData { get; }

            private AuthorizerDefinition(AuthorizeWhenAttribute attr)
            {
                Authorizer = attr.authorizerType;
                CustomData = attr.customData;
            }

            internal static AuthorizerDefinition Create(AuthorizeWhenAttribute attr) =>
                new AuthorizerDefinition(attr);
        }
    }
}
