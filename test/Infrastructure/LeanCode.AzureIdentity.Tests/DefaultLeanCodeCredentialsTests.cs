using Azure.Core;
using Xunit;

namespace LeanCode.AzureIdentity.Tests
{
    public class DefaultLeanCodeCredentialsTests
    {
        [AzureIdentityFact("Azure__UseAzureCLI")]
        public async Task Authorizes_via_cli()
        {
            var cred = DefaultLeanCodeCredential.Create(new AzureCredentialConfiguration
            {
                UseAzureCLI = true,
            });

            await AssertGetsTokenAsync(cred);
        }

        [AzureIdentityFact("Azure__TenantId", "Azure__ClientId", "Azure__ClientSecret")]
        public async Task Authorizes_using_service_principal_secret()
        {
            var config = new AzureCredentialConfiguration
            {
                TenantId = Environment.GetEnvironmentVariable("Azure__TenantId"),
                ClientId = Environment.GetEnvironmentVariable("Azure__ClientId"),
                ClientSecret = Environment.GetEnvironmentVariable("Azure__ClientSecret"),
            };

            var cred = DefaultLeanCodeCredential.Create(config);

            await AssertGetsTokenAsync(cred);
        }

        // this one won't be of much use unless we run tests on Azure VM
        [AzureIdentityFact("Azure__UseManagedIdentity")]
        public async Task Authorizes_via_managed_identity()
        {
            var cred = DefaultLeanCodeCredential.Create(new AzureCredentialConfiguration
            {
                UseManagedIdentity = true,
            });

            await AssertGetsTokenAsync(cred);
        }

        [Fact]
        public void Throws_if_no_authorization_method_specified()
        {
            Assert.Throws<InvalidOperationException>(() => DefaultLeanCodeCredential.Create(new AzureCredentialConfiguration()));
        }

        [Fact]
        public void Throws_if_more_than_one_authorization_method_specified()
        {
            var config = new AzureCredentialConfiguration
            {
                UseManagedIdentity = true,
                UseAzureCLI = true,
            };

            Assert.Throws<InvalidOperationException>(() => DefaultLeanCodeCredential.Create(config));
        }

        private static async Task AssertGetsTokenAsync(TokenCredential cred)
        {
            var ex = await Record.ExceptionAsync(() => cred.GetTokenAsync(
                new TokenRequestContext(new[] { "https://database.windows.net/.default" }), // just an arbitrary scope
                CancellationToken.None)
                .AsTask());

            Assert.Null(ex);
        }
    }
}
