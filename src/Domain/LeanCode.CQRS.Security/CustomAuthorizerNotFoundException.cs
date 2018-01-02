using System;

namespace LeanCode.CQRS.Security
{
    public class CustomAuthorizerNotFoundException : Exception
    {
        public Type AuthorizerType { get; }

        public CustomAuthorizerNotFoundException(Type authorizerType)
            : base($"Cannot find custom authorizer for {authorizerType.Name}.")
        {
            AuthorizerType = authorizerType;
        }
    }
}
