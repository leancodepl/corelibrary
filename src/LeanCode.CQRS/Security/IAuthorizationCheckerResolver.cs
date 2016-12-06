using System;

namespace LeanCode.CQRS.Security
{
    public interface IAuthorizationCheckerResolver
    {
        ICustomAuthorizationChecker FindAuthorizationChecker(Type type);
    }
}
