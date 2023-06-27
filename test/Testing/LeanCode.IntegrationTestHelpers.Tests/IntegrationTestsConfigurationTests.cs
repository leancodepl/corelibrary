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

        var configurationOverrides = new ConfigurationOverrides(
            connectionStringKey: customConnectionStringKey,
            connectionStringBase: customConnectionStringBaseKey
        );

        var config = new ConfigurationBuilder().Add(configurationOverrides).Build();

        config.GetValue<string>(customConnectionStringKey).Should().NotBeEmpty();
        config.GetValue<string>(customConnectionStringBaseKey).Should().NotBeEmpty();
    }
}
