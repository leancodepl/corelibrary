using System;

namespace LeanCode.ExternalIdentityProviders;

[System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1034", Justification = "C# way for union types.")]
public abstract record SignInResult
{
    private SignInResult() { }

    public sealed record Success(Guid UserId) : SignInResult;

    public sealed record TokenIsInvalid : SignInResult;

    public sealed record UserDoesNotExist : SignInResult;
}
