using Microsoft.Extensions.Configuration;
using Xunit;

namespace LeanCode.IntegrationTestHelpers.Tests;

public class ConfigurationOverridesTests
{
    private const string ConnectionStringKey = "ConnectionStrings:DbContext";
    private const string ConnectionStringKey1 = "ConnectionStrings:DbContext1";
    private const string ConnectionStringKey2 = "ConnectionStrings:DbContext2";
    private const string DbPrefix = "prefix";
    private const string DbPrefix1 = "prefix1";
    private const string DbPrefix2 = "prefix2";
    private const string ExpectedSecondPartOfConnectionString =
        $"Server=(local);Database=DbContext;Trusted_Connection=True;";
    private const string ExpectedSecondPartOfConnectionString1 =
        $"Server=(local);Database=DbContext1;Trusted_Connection=True;";
    private const string ExpectedSecondPartOfConnectionString2 =
        $"Server=(local);Database=DbContext2;Trusted_Connection=True;";

    IConfigurationRoot BuildConfiguration(ConfigurationOverrides overrides)
    {
        var configuration = new ConfigurationBuilder().Add(overrides).Build();

        return configuration;
    }

    [Fact]
    public void Empty_ConfigurationOverrides_results_in_empty_Configuration_test()
    {
        var configuration = BuildConfiguration(new ConfigurationOverrides());

        Assert.Empty(configuration.AsEnumerable());
    }

    [Fact]
    public void Adding_single_value_results_in_single_value_in_configration_test()
    {
        var overrides = new ConfigurationOverrides().Add("key", "value");

        var configuration = BuildConfiguration(overrides);

        Assert.Equal("value", configuration["key"]);
    }

    [Fact]
    public void Adding_multiple_values_results_in_multiple_values_in_configration_test()
    {
        var overrides = new ConfigurationOverrides().Add("key1", "value1").Add("key2", "value2").Add("key3", "value3");

        var configuration = BuildConfiguration(overrides);

        Assert.Equal("value1", configuration["key1"]);
        Assert.Equal("value2", configuration["key2"]);
        Assert.Equal("value3", configuration["key3"]);
    }

    [Fact]
    public void Adding_multiple_values_with_the_same_key_overrides_value_test()
    {
        var overrides = new ConfigurationOverrides().Add("key", "value1").Add("key", "value2").Add("key", "value3");

        var configuration = BuildConfiguration(overrides);

        Assert.Equal("value3", configuration["key"]);
    }

    [Fact]
    public void Adding_single_ConnectionString_results_in_single_ConnectionString_in_configration_test()
    {
        var expectedDbName = $"{DbPrefix}_";
        var expectedFirstPartOfConnectionString = $"Database={expectedDbName}";

        Environment.SetEnvironmentVariable(
            ConnectionStringKey,
            "Server=(local);Database=DbContext;Trusted_Connection=True;"
        );

        var overrides = new ConfigurationOverrides().AddConnectionString(
            "SqlServer:ConnectionString",
            DbPrefix,
            ConnectionStringKey
        );

        var configuration = BuildConfiguration(overrides);

        Assert.NotNull(configuration["SqlServer:ConnectionString"]);

        Assert.StartsWith(
            expectedFirstPartOfConnectionString,
            configuration["SqlServer:ConnectionString"],
            System.StringComparison.Ordinal
        );

        Assert.EndsWith(
            ExpectedSecondPartOfConnectionString,
            configuration["SqlServer:ConnectionString"],
            System.StringComparison.Ordinal
        );
    }

    [Fact]
    public void Adding_multiple_ConnectionStrings_results_in_multiple_ConnectionStrings_in_configration_test()
    {
        var expectedDbName1 = $"{DbPrefix1}_";
        var expectedDbName2 = $"{DbPrefix2}_";

        var expectedFirstPartOfConnectionString1 = $"Database={expectedDbName1}";
        var expectedFirstPartOfConnectionString2 = $"Database={expectedDbName2}";

        Environment.SetEnvironmentVariable(
            ConnectionStringKey1,
            "Server=(local);Database=DbContext1;Trusted_Connection=True;"
        );

        Environment.SetEnvironmentVariable(
            ConnectionStringKey2,
            "Server=(local);Database=DbContext2;Trusted_Connection=True;"
        );

        var overrides = new ConfigurationOverrides()
            .AddConnectionString("SqlServer:FirstConnectionString", DbPrefix1, ConnectionStringKey1)
            .AddConnectionString("SqlServer:SecondConnectionString", DbPrefix2, ConnectionStringKey2);

        var configuration = BuildConfiguration(overrides);

        Assert.NotNull(configuration["SqlServer:FirstConnectionString"]);
        Assert.NotNull(configuration["SqlServer:SecondConnectionString"]);

        Assert.StartsWith(
            expectedFirstPartOfConnectionString1,
            configuration["SqlServer:FirstConnectionString"],
            System.StringComparison.Ordinal
        );
        Assert.StartsWith(
            expectedFirstPartOfConnectionString2,
            configuration["SqlServer:SecondConnectionString"],
            System.StringComparison.Ordinal
        );

        Assert.EndsWith(
            ExpectedSecondPartOfConnectionString1,
            configuration["SqlServer:FirstConnectionString"],
            System.StringComparison.Ordinal
        );
        Assert.EndsWith(
            ExpectedSecondPartOfConnectionString2,
            configuration["SqlServer:SecondConnectionString"],
            System.StringComparison.Ordinal
        );
    }

    [Fact]
    public void Adding_single_ConnectionString_and_single_Value_results_in_single_ConnectionString_and_single_Value_in_configration_test()
    {
        var expectedDbName = $"{DbPrefix}_";
        var expectedFirstPartOfConnectionString = $"Database={expectedDbName}";

        Environment.SetEnvironmentVariable(
            ConnectionStringKey,
            "Server=(local);Database=DbContext;Trusted_Connection=True;"
        );

        var overrides = new ConfigurationOverrides()
            .AddConnectionString("SqlServer:ConnectionString", DbPrefix, ConnectionStringKey)
            .Add("key", "value");

        var configuration = BuildConfiguration(overrides);

        Assert.Equal("value", configuration["key"]);

        Assert.NotNull(configuration["SqlServer:ConnectionString"]);

        Assert.StartsWith(
            expectedFirstPartOfConnectionString,
            configuration["SqlServer:ConnectionString"],
            System.StringComparison.Ordinal
        );

        Assert.EndsWith(
            ExpectedSecondPartOfConnectionString,
            configuration["SqlServer:ConnectionString"],
            System.StringComparison.Ordinal
        );
    }
}
