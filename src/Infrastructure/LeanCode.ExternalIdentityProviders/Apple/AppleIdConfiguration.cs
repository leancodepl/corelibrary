using System.Collections.Immutable;

namespace LeanCode.ExternalIdentityProviders.Apple;

public sealed record AppleIdConfiguration(ImmutableList<string> ClientIds)
{
    public AppleIdConfiguration(string clientId)
        : this(ImmutableList.Create(clientId))
    { }
}
