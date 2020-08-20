using System;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace LeanCode.Components.Startup
{
    public static class IConfigurationBuilderExtensions
    {
        public const string VaultKey = "Secrets:KeyVault:VaultUrl";
        public const string ClientIdKey = "Secrets:KeyVault:ClientId";
        public const string ClientSecretKey = "Secrets:KeyVault:ClientSecret";
        public const string TenantIdKey = "Secrets:KeyVault:TenantId";

        public static IConfigurationBuilder AddAppConfigurationFromAzureKeyVault(this IConfigurationBuilder builder)
        {
            var configuration = builder.Build();

            var vault = configuration.GetValue<string?>(VaultKey);
            var tenantId = configuration.GetValue<string?>(TenantIdKey);
            var clientId = configuration.GetValue<string?>(ClientIdKey);
            var clientSecret = configuration.GetValue<string?>(ClientSecretKey);

            if (vault != null && tenantId != null && clientId != null && clientSecret != null)
            {
                var vaultUrl = new Uri(vault);
                var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                return builder.AddAzureKeyVault(vaultUrl, clientSecretCredential);
            }
            else
            {
                throw new ApplicationException("Application startup exception: null key vault credentials.");
            }
        }
    }
}
