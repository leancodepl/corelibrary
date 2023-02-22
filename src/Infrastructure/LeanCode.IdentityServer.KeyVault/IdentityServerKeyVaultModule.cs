using Autofac;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using LeanCode.Components;
using Microsoft.Extensions.Azure;

namespace LeanCode.IdentityServer.KeyVault;

public class IdentityServerKeyVaultModule : AppModule
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<KeyMaterialService>().AsImplementedInterfaces();
        builder.RegisterType<TokenCreationService>().AsImplementedInterfaces();
        builder
            .Register(ctx =>
            {
                var factory = ctx.Resolve<IAzureClientFactory<CryptographyClient>>();
                return new SigningService(
                    ctx.Resolve<KeyClient>(),
                    factory.CreateClient(AzureClientFactoryBuilderExtensions.TokenSigningKeyClientName)
                );
            })
            .AsSelf()
            .SingleInstance();
    }
}
