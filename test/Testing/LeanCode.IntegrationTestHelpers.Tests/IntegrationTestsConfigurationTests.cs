using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LeanCode.IntegrationTestHelpers.Tests;

public class IntegrationTestsConfigurationTests
{
    [Fact]
    public void Custom_configuration_keys_are_respected()
    {
        var customConnectionStringKey = "CustomConnectionStringKey";
        var customConnectionStringBaseKey = "CustomConnectionStringBaseKey";
        var customInternalBaseKey = "CustomInternalBaseKey";
        var customPublicBaseKey = "CustomPublicBaseKey";

        var configurationOverrides = new ConfigurationOverrides(
            connectionStringKey: customConnectionStringKey,
            connectionStringBase: customConnectionStringBaseKey,
            internalBaseKey: customInternalBaseKey,
            publicBaseKey: customPublicBaseKey
        );

        var config = new ConfigurationBuilder().Add(configurationOverrides).Build();

        config.GetValue<string>(customConnectionStringKey).Should().NotBeEmpty();
        config.GetValue<string>(customConnectionStringBaseKey).Should().NotBeEmpty();
        config.GetValue<string>(customInternalBaseKey).Should().NotBeEmpty();
        config.GetValue<string>(customPublicBaseKey).Should().NotBeEmpty();
    }
}
