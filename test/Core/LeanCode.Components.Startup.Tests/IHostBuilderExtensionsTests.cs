using System.Collections.Generic;
using System.Linq;
using LeanCode.Components.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace LeanCode.Components.Tests
{
    public class IHostBuilderExtensionsTests
    {
        [Fact]
        public void Configuring_Azure_Key_Vault_credentials_with_custom_keys_does_not_throw()
        {
            var vaultKeyOverride = "VaultKeyOverride";
            var tenantIdKeyOverride = "TenantIdKeyOverride";
            var clientIdKeyOverride = "ClientIdKeyOverride";
            var clientSecretKeyOverride = "ClientSecretKeyOverride";

            var hostBuilder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.Add(new AzureConfigOverrides(new(
                        vaultKeyOverride,
                        tenantIdKeyOverride,
                        clientIdKeyOverride,
                        clientSecretKeyOverride)));
                }).AddAppConfigurationFromAzureKeyVault(
                    vaultKeyOverride,
                    tenantIdKeyOverride,
                    clientIdKeyOverride,
                    clientSecretKeyOverride);
        }

        private class AzureConfigOverrides : IConfigurationSource
        {
            private readonly CustomAzureKVConfigProvider provider;

            public AzureConfigOverrides(CustomAzureKVConfigProvider provider)
            {
                this.provider = provider;
            }

            public IConfigurationProvider Build(IConfigurationBuilder builder) => provider;
        }

        private class CustomAzureKVConfigProvider : ConfigurationProvider
        {
            private readonly List<string> azureKVKeys;

            public CustomAzureKVConfigProvider(
                string vaultKeyOverride,
                string tenantIdKeyOverride,
                string clientIdKeyOverride,
                string clientSecretKeyOverride)
            {
                azureKVKeys = new()
                {
                    vaultKeyOverride, tenantIdKeyOverride, clientIdKeyOverride, clientSecretKeyOverride,
                };
            }

            public override void Load()
            {
                Data = azureKVKeys.ToDictionary(k => k, _ => "SomeValue");
            }
        }
    }
}
