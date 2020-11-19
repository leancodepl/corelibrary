namespace LeanCode.ExternalIdentityProviders.Apple
{
    public sealed record AppleUser(string Id, string? Email, bool EmailConfirmed, string? Picture);
}
