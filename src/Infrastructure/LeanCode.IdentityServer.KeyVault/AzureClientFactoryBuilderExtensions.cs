using Azure.Core.Extensions;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.Extensions.Azure;

namespace LeanCode.IdentityServer.KeyVault;

public static class AzureClientFactoryBuilderExtensions
{
    public const string TokenSigningKeyClientName = "identityserver.tokensigning";

    public static IAzureClientBuilder<CryptographyClient, CryptographyClientOptions> AddIdentityServerTokenSigningKey(
        this AzureClientFactoryBuilder builder,
        Uri keyUrl
    )
    {
        return builder.AddCryptographyClient(keyUrl).WithName(TokenSigningKeyClientName);
    }
}
