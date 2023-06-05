using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace LeanCode.AzureIdentity;

public static class IHostBuilderExtensions
{
    public const string VaultUrlKey = "KeyVault:VaultUrl";

    public static IHostBuilder AddAppConfigurationFromAzureKeyVault(
        this IHostBuilder builder,
        TokenCredential? credential = null,
        string? keyVaultKeyOverride = null,
        KeyVaultSecretManager? manager = null
    )
    {
        return builder.ConfigureAppConfiguration(
            (context, builder) =>
            {
                ConfigureAzureKeyVault(builder, credential, keyVaultKeyOverride, manager);
            }
        );
    }

    public static IHostBuilder AddAppConfigurationFromAzureKeyVaultOnNonDevelopmentEnvironment(
        this IHostBuilder builder,
        TokenCredential? credential = null,
        string? keyVaultKeyOverride = null,
        KeyVaultSecretManager? manager = null
    )
    {
        return builder.ConfigureAppConfiguration(
            (context, builder) =>
            {
                if (!context.HostingEnvironment.IsDevelopment())
                {
                    ConfigureAzureKeyVault(builder, credential, keyVaultKeyOverride, manager);
                }
            }
        );
    }

    private static void ConfigureAzureKeyVault(
        IConfigurationBuilder builder,
        TokenCredential? credential,
        string? keyVaultUrlKeyOverride,
        KeyVaultSecretManager? manager
    )
    {
        var configuration = builder.Build();

        var vault = configuration.GetValue<string?>(keyVaultUrlKeyOverride ?? VaultUrlKey);
        if (vault != null)
        {
            var vaultUrl = new Uri(vault);
            credential ??= DefaultLeanCodeCredential.Create(configuration);
            if (manager is not null)
            {
                builder.AddAzureKeyVault(vaultUrl, credential, manager);
            }
            else
            {
                builder.AddAzureKeyVault(vaultUrl, credential);
            }
        }
        else
        {
            throw new ArgumentException("Application startup exception: null key vault address.");
        }
    }
}
