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

        var configurationOverrides = new ConfigurationOverrides()
            .AddConnectionString(customConnectionStringKey, "CustomConnectionStringValue")
            .Add(customInternalBaseKey, "CustomInternalBaseValue")
            .Add(customPublicBaseKey, "CustomPublicBaseValue");

        var config = new ConfigurationBuilder().Add(configurationOverrides).Build();

        Assert.NotEmpty(config.GetValue<string>(customConnectionStringKey)!);
        Assert.NotEmpty(config.GetValue<string>(customInternalBaseKey)!);
        Assert.NotEmpty(config.GetValue<string>(customPublicBaseKey)!);
    }
}
