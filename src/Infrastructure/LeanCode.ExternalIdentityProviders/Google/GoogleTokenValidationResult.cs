namespace LeanCode.ExternalIdentityProviders.Google
{
    public abstract record GoogleTokenValidationResult
    {
        private GoogleTokenValidationResult() { }

        public sealed record Success(GoogleUser User) : GoogleTokenValidationResult;

        public sealed record Failure : GoogleTokenValidationResult;
    }
}
