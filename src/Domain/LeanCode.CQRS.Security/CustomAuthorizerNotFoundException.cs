using System;

namespace LeanCode.CQRS.Security
{
    public class CustomAuthorizerNotFoundException : Exception
    {
        public Type ContextType { get; }
        public Type AuthorizerType { get; }

        public CustomAuthorizerNotFoundException(Type contextType, Type authorizerType)
            : base($"Cannot find custom authorizer for {authorizerType.Name} executed with context {contextType.Name}.")
        {
            ContextType = contextType;
            AuthorizerType = authorizerType;
        }
    }
}
