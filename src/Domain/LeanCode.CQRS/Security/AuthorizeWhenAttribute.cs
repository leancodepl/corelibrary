using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LeanCode.CQRS.Security
{
    /// <summary>
    /// Base attribute for defining command/query authorization rules
    /// </summary>
    /// <remarks>
    /// If an object is decorated with multiple <c>AuthorizeWhen</c> attributes, all of them must succeed to
    /// authorize a user (as in AND clause)
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public class AuthorizeWhenAttribute : Attribute
    {
        private readonly Type authorizerType;
        private readonly object customData;

        public AuthorizeWhenAttribute(Type authorizerType, object customData = null)
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
            public object CustomData { get; }

            private AuthorizerDefinition(AuthorizeWhenAttribute attr)
            {
                this.Authorizer = attr.authorizerType;
                this.CustomData = attr.customData;
            }

            internal static AuthorizerDefinition Create(
                AuthorizeWhenAttribute attr)
            {
                return new AuthorizerDefinition(attr);
            }
        }
    }
}
