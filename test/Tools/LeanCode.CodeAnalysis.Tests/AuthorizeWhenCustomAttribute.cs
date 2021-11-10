using LeanCode.CQRS.Security;

namespace LeanCode.CodeAnalysis.Tests
{
    public sealed class AuthorizeWhenCustomAttribute : AuthorizeWhenAttribute
    {
        public AuthorizeWhenCustomAttribute(Type authorizerType = null)
               : base(authorizerType ?? typeof(object))
        { }
    }
}
