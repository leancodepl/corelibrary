namespace LeanCode.ExternalIdentityProviders.Google;

[System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1034", Justification = "C# way for union types.")]
public abstract record GoogleTokenValidationResult
{
    private GoogleTokenValidationResult() { }

    public sealed record Success(GoogleUser User) : GoogleTokenValidationResult;

    public sealed record Failure : GoogleTokenValidationResult;
}
