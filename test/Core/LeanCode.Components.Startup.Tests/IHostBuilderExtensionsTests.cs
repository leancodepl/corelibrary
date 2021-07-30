using System;
using System.Collections.Generic;
using LeanCode.Components.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace LeanCode.Components.Tests
{
    public class IHostBuilderExtensionsTests
    {
        [Fact]
        public void Configuring_Azure_Key_Vault_credentials_with_custom_keys_works()
        {
            const string vaultKeyOverride = "VaultKeyOverride";
            const string tenantIdKeyOverride = "TenantIdKeyOverride";
            const string clientIdKeyOverride = "ClientIdKeyOverride";
            const string clientSecretKeyOverride = "ClientSecretKeyOverride";

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

            var e = Assert.Throws<AggregateException>(() => hostBuilder.Build());

            Assert.All(
                e.InnerExceptions,
                e => Assert.Contains(
                    "azurekeyvault.local.lncd.pl",
                    Assert.IsType<Azure.RequestFailedException>(e).Message));
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
            private readonly string vaultKeyOverride;
            private readonly string tenantIdKeyOverride;
            private readonly string clientIdKeyOverride;
            private readonly string clientSecretKeyOverride;

            public CustomAzureKVConfigProvider(
                string vaultKeyOverride,
                string tenantIdKeyOverride,
                string clientIdKeyOverride,
                string clientSecretKeyOverride)
            {
                this.vaultKeyOverride = vaultKeyOverride;
                this.tenantIdKeyOverride = tenantIdKeyOverride;
                this.clientIdKeyOverride = clientIdKeyOverride;
                this.clientSecretKeyOverride = clientSecretKeyOverride;
            }

            public override void Load()
            {
                Data = new Dictionary<string, string>
                {
                    [vaultKeyOverride] = "https://azurekeyvault.local.lncd.pl/keys/token-signing",
                    [tenantIdKeyOverride] = Guid.Empty.ToString(),
                    [clientIdKeyOverride] = Guid.Empty.ToString(),
                    [clientSecretKeyOverride] = "secret",
                };
            }
        }
    }
}
