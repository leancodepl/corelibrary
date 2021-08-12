using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace LeanCode.AzureIdentity;

public static class DefaultLeancodeCredential
{
    public static TokenCredential Create(IConfiguration configuration)
    {
        var config = new AzureCredentialConfiguration
        {
            TenantId = Get<string>(AzureCredentialConfiguration.TenantIdKey),
            ClientId = Get<string>(AzureCredentialConfiguration.TenantIdKey),
            ClientSecret = Get<string>(AzureCredentialConfiguration.TenantIdKey),
            UseAzureCLI = Get<bool>(AzureCredentialConfiguration.UseAzureCLIKey),
            UseManagedIdentity = Get<bool>(AzureCredentialConfiguration.UseManagedIdentityKey),
        };

        return Create(config);

        T Get<T>(string key) => configuration.GetValue<T>(key);
    }

    public static TokenCredential CreateFromEnvironment()
    {
        var config = new AzureCredentialConfiguration
        {
            TenantId = GetEnv(AzureCredentialConfiguration.TenantIdKey),
            ClientId = GetEnv(AzureCredentialConfiguration.TenantIdKey),
            ClientSecret = GetEnv(AzureCredentialConfiguration.TenantIdKey),
            UseAzureCLI = GetEnv(AzureCredentialConfiguration.UseAzureCLIKey) is string cli && bool.Parse(cli),
            UseManagedIdentity = GetEnv(AzureCredentialConfiguration.UseManagedIdentityKey) is string mi && bool.Parse(mi),
        };

        return Create(config);

        static string? GetEnv(string rawKey) => Environment.GetEnvironmentVariable(rawKey.Replace(":", "__"));
    }

    public static TokenCredential Create(AzureCredentialConfiguration config)
    {
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
            if (string.IsNullOrEmpty(config.TenantId) ||
                string.IsNullOrEmpty(config.ClientId) ||
                string.IsNullOrEmpty(config.ClientSecret))
            {
                throw new InvalidOperationException("Missing Azure Identity configuration");
            }

            return new ClientSecretCredential(config.TenantId, config.ClientId, config.ClientSecret);
        }
    }
}
