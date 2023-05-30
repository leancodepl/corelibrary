using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using IdentityServer4.Services;
using LeanCode.Components;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.IdentityServer.KeyVault;

public class IdentityServerKeyVaultModule : AppModule
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddTransient<IKeyMaterialService, KeyMaterialService>();
        services.TryAddSingleton<ITokenCreationService, TokenCreationService>();
        services.TryAddSingleton(
            ctx =>
                new SigningService(
                    ctx.GetRequiredService<KeyClient>(),
                    ctx.GetRequiredService<IAzureClientFactory<CryptographyClient>>()
                        .CreateClient(AzureClientFactoryBuilderExtensions.TokenSigningKeyClientName)
                )
        );
    }
}
