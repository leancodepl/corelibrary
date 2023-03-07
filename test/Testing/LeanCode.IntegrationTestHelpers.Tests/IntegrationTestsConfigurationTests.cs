using Microsoft.Extensions.Configuration;
using Xunit;

namespace LeanCode.IntegrationTestHelpers.Tests;

public class IntegrationTestsConfigurationTests
{
    [Fact]
    public void Custom_configuration_keys_are_respected()
    {
        var customConnectionStringKey = "CustomConnectionStringKey";
        var customInternalBaseKey = "CustomInternalBaseKey";
        var customPublicBaseKey = "CustomPublicBaseKey";

        var configurationOverrides = new ConfigurationOverrides(
            connectionStringKey: customConnectionStringKey,
            internalBaseKey: customInternalBaseKey,
            publicBaseKey: customPublicBaseKey
        );

        var config = new ConfigurationBuilder().Add(configurationOverrides).Build();

        Assert.NotEmpty(config.GetValue<string>(customConnectionStringKey) ?? string.Empty);
        Assert.NotEmpty(config.GetValue<string>(customInternalBaseKey) ?? string.Empty);
        Assert.NotEmpty(config.GetValue<string>(customPublicBaseKey) ?? string.Empty);
    }
}
