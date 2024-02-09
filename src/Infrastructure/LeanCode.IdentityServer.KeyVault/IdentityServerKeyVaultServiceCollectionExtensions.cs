using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using IdentityServer4.Services;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.IdentityServer.KeyVault;

public static class IdentityServerKeyVaultServiceCollectionExtensions
{
    private const string TokenSigningKeyClientName = "identityserver.tokensigning";

    public static IServiceCollection AddIdentityServerKeyVaultTokenSigning(
        this IServiceCollection services,
        Uri signingKeyUrl
    )
    {
        services.TryAddTransient<IKeyMaterialService, KeyMaterialService>();
        services.TryAddSingleton<ITokenCreationService, TokenCreationService>();
        services.TryAddSingleton(ctx => new SigningService(
            ctx.GetRequiredService<KeyClient>(),
            ctx.GetRequiredService<IAzureClientFactory<CryptographyClient>>().CreateClient(TokenSigningKeyClientName)
        ));

        services.AddAzureClients(cfg =>
        {
            cfg.AddCryptographyClient(signingKeyUrl).WithName(TokenSigningKeyClientName);
        });

        return services;
    }
}
