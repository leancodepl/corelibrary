using System;

namespace LeanCode.ExternalIdentityProviders
{
    public abstract record SignInResult
    {
        private SignInResult() { }

        public sealed record Success(Guid UserId) : SignInResult;

        public sealed record TokenIsInvalid : SignInResult;

        public sealed record UserDoesNotExist : SignInResult;
    }
}
