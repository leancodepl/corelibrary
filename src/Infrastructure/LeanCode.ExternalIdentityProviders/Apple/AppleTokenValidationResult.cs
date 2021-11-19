namespace LeanCode.ExternalIdentityProviders.Apple
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1034", Justification = "C# way for union types.")]
    public abstract record AppleTokenValidationResult
    {
        private AppleTokenValidationResult() { }

        public sealed record Success(AppleUser User) : AppleTokenValidationResult;

        public sealed record Failure : AppleTokenValidationResult;
    }
}
