using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LeanCode.AzureIdentity.Tests;

public sealed class DefaultLeanCodeCredentialsTests_Configuration : IDisposable
{
    [Fact]
    public void Reads_config_from_asp_net_config()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string>
                {
                    [AzureCredentialConfiguration.TenantIdKey] = Guid.NewGuid().ToString(),
                    [AzureCredentialConfiguration.ClientIdKey] = Guid.NewGuid().ToString(),
                    [AzureCredentialConfiguration.ClientSecretKey] = "client_secret",
                }
            )
            .Build();

        var credential = DefaultLeanCodeCredential.Create(config);
        Assert.IsType<ClientSecretCredential>(credential);
    }

    [Fact]
    public void Reads_config_from_env_variables()
    {
        // This is overridden so that it does not interfere with tests actually using the variable.
        AzureCredentialConfiguration.UseAzureCLIKey = "Test__Azure__UseAzureCLI";
        AzureCredentialConfiguration.UseManagedIdentityKey = "Test__Azure__UseManagedIdentity";
        AzureCredentialConfiguration.ClientSecretKey = "Test__Azure__ClientSecret";

        Environment.SetEnvironmentVariable("Test__Azure__UseAzureCLI", "true");

        var credential = DefaultLeanCodeCredential.CreateFromEnvironment();

        Assert.IsType<AzureCliCredential>(credential);
    }

    [Fact]
    public void Throws_if_no_authorization_method_specified()
    {
        Assert.Throws<InvalidOperationException>(
            () => DefaultLeanCodeCredential.Create(new AzureCredentialConfiguration())
        );
    }

    [Fact]
    public void Throws_if_more_than_one_authorization_method_specified()
    {
        var config = new AzureCredentialConfiguration { UseManagedIdentity = true, UseAzureCLI = true, };

        Assert.Throws<InvalidOperationException>(() => DefaultLeanCodeCredential.Create(config));
    }

    public void Dispose()
    {
        AzureCredentialConfiguration.UseAzureCLIKey = "Azure__UseAzureCLI";
        AzureCredentialConfiguration.UseManagedIdentityKey = "Azure__UseManagedIdentity";
        AzureCredentialConfiguration.ClientSecretKey = "Azure__ClientSecret";
    }
}
