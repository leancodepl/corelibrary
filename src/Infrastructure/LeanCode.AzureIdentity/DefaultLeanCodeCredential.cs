using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace LeanCode.AzureIdentity;

public static class DefaultLeanCodeCredential
{
    public static TokenCredential Create(IConfiguration configuration)
    {
        var config = new AzureCredentialConfiguration
        {
            TenantId = Get<string>(AzureCredentialConfiguration.TenantIdKey),
            ClientId = Get<string>(AzureCredentialConfiguration.ClientIdKey),
            ClientSecret = Get<string>(AzureCredentialConfiguration.ClientSecretKey),
            UseAzureCLI = Get<bool>(AzureCredentialConfiguration.UseAzureCLIKey),
            UseManagedIdentity = Get<bool>(AzureCredentialConfiguration.UseManagedIdentityKey),
        };

        return Create(config);

        T? Get<T>(string key) => configuration.GetValue<T>(key);
    }

    public static TokenCredential CreateFromEnvironment()
    {
        var config = new AzureCredentialConfiguration
        {
            TenantId = GetEnv(AzureCredentialConfiguration.TenantIdKey),
            ClientId = GetEnv(AzureCredentialConfiguration.ClientIdKey),
            ClientSecret = GetEnv(AzureCredentialConfiguration.ClientSecretKey),
            UseAzureCLI = GetEnv(AzureCredentialConfiguration.UseAzureCLIKey) is string cli && bool.Parse(cli),
            UseManagedIdentity =
                GetEnv(AzureCredentialConfiguration.UseManagedIdentityKey) is string mi && bool.Parse(mi),
        };

        return Create(config);

        static string? GetEnv(string rawKey) =>
            Environment.GetEnvironmentVariable(rawKey.Replace(":", "__", StringComparison.Ordinal));
    }

    public static TokenCredential Create(AzureCredentialConfiguration config)
    {
        Validate(config);

        if (config.UseManagedIdentity)
        {
            return new ManagedIdentityCredential(config.ClientId);
        }
        else if (config.UseAzureCLI)
        {
            return new AzureCliCredential();
        }
        else
        {
            if (
                string.IsNullOrEmpty(config.TenantId)
                || string.IsNullOrEmpty(config.ClientId)
                || string.IsNullOrEmpty(config.ClientSecret)
            )
            {
                throw new InvalidOperationException("Missing Azure Identity configuration.");
            }

            return new ClientSecretCredential(config.TenantId, config.ClientId, config.ClientSecret);
        }
    }

    private static void Validate(AzureCredentialConfiguration config)
    {
        var methodsUsed =
            (config.UseAzureCLI ? 1 : 0)
            + (config.UseManagedIdentity ? 1 : 0)
            + (!string.IsNullOrWhiteSpace(config.ClientSecret) ? 1 : 0);

        if (methodsUsed != 1)
        {
            throw new InvalidOperationException(
                "You need to specify exactly one authorization method: Azure CLI, Managed Identity or Client Secret."
            );
        }
    }
}
