using System.Collections.Immutable;

namespace LeanCode.ExternalIdentityProviders.Google
{
    public sealed record GoogleAuthConfiguration(ImmutableList<string> ClientIds)
    {
        public GoogleAuthConfiguration(string clientId)
            : this(ImmutableList.Create(clientId))
        { }
    }
}
