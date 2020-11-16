namespace LeanCode.ExternalIdentityProviders.Apple
{
    public abstract record AppleTokenValidationResult
    {
        private AppleTokenValidationResult() { }

        public sealed record Success(AppleUser User) : AppleTokenValidationResult;

        public sealed record Failure : AppleTokenValidationResult;
    }
}
