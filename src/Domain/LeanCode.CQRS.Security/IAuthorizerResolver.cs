using System;

namespace LeanCode.CQRS.Security
{
    public interface IAuthorizerResolver
    {
        ICustomAuthorizer FindAuthorizer(Type type);
    }
}
