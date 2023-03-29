using Microsoft.Extensions.Configuration;
using Xunit;

namespace LeanCode.IntegrationTestHelpers.Tests;

public class ConfigurationOverridesTests
{
    [Fact]
    public void Empty_ConfigurationOverrides_Results_In_Empty_Configuration_test()
    {
        var configuration = new ConfigurationBuilder().Add(new ConfigurationOverrides()).Build();

        Assert.Empty(configuration.AsEnumerable());
    }

    [Fact]
    public void ConfigurationOverrides_single_Add_method_test()
    {
        var overrides = new ConfigurationOverrides().Add("key", "value");

        var configuration = new ConfigurationBuilder().Add(overrides).Build();

        Assert.Equal("value", configuration["key"]);
    }

    [Fact]
    public void ConfigurationOverrides_multiple_Add_method_test()
    {
        var overrides = new ConfigurationOverrides().Add("key1", "value1").Add("key2", "value2").Add("key3", "value3");

        var configuration = new ConfigurationBuilder().Add(overrides).Build();

        Assert.Equal("value1", configuration["key1"]);
        Assert.Equal("value2", configuration["key2"]);
        Assert.Equal("value3", configuration["key3"]);
    }

    [Fact]
    public void ConfigurationOverrides_Add_method_called_multiple_times_with_the_same_key_test()
    {
        var overrides = new ConfigurationOverrides().Add("key", "value1").Add("key", "value2").Add("key", "value3");

        var configuration = new ConfigurationBuilder().Add(overrides).Build();

        Assert.Equal("value3", configuration["key"]);
    }

    [Fact]
    public void ConfigurationOverrides_single_AddConnectionString_method_test()
    {
        var connectionStringKey = "ConnectionStrings:DbContext";
        var dbPrefix = "prefix";

        var expectedDbName = $"{dbPrefix}_";
        var expectedFirstPartOfConnectionString = $"Database={expectedDbName}";
        var expectedSecondPartOfConnectionString = $"Server=(local);Database=DbContext;Trusted_Connection=True;";

        Environment.SetEnvironmentVariable(
            connectionStringKey,
            "Server=(local);Database=DbContext;Trusted_Connection=True;"
        );

        var overrides = new ConfigurationOverrides().AddConnectionString(
            "SqlServer:ConnectionString",
            dbPrefix,
            connectionStringKey
        );

        var configuration = new ConfigurationBuilder().Add(overrides).Build();

        Assert.NotNull(configuration["SqlServer:ConnectionString"]);

        Assert.StartsWith(
            expectedFirstPartOfConnectionString,
            configuration["SqlServer:ConnectionString"],
            System.StringComparison.Ordinal
        );

        Assert.EndsWith(
            expectedSecondPartOfConnectionString,
            configuration["SqlServer:ConnectionString"],
            System.StringComparison.Ordinal
        );
    }

    [Fact]
    public void ConfigurationOverrides_multiple_AddConnectionString_method_test()
    {
        var connectionStringKey1 = "ConnectionStrings:DbContext1";
        var connectionStringKey2 = "ConnectionStrings:DbContext2";

        var dbPrefix1 = "prefix1";
        var dbPrefix2 = "prefix2";

        var expectedDbName1 = $"{dbPrefix1}_";
        var expectedDbName2 = $"{dbPrefix2}_";

        var expectedFirstPartOfConnectionString1 = $"Database={expectedDbName1}";
        var expectedFirstPartOfConnectionString2 = $"Database={expectedDbName2}";

        var expectedSecondPartOfConnectionString1 = $"Server=(local);Database=DbContext1;Trusted_Connection=True;";
        var expectedSecondPartOfConnectionString2 = $"Server=(local);Database=DbContext2;Trusted_Connection=True;";

        Environment.SetEnvironmentVariable(
            connectionStringKey1,
            "Server=(local);Database=DbContext1;Trusted_Connection=True;"
        );

        Environment.SetEnvironmentVariable(
            connectionStringKey2,
            "Server=(local);Database=DbContext2;Trusted_Connection=True;"
        );

        var overrides = new ConfigurationOverrides()
            .AddConnectionString("SqlServer:FirstConnectionString", dbPrefix1, connectionStringKey1)
            .AddConnectionString("SqlServer:SecondConnectionString", dbPrefix2, connectionStringKey2);

        var configuration = new ConfigurationBuilder().Add(overrides).Build();

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
            expectedSecondPartOfConnectionString1,
            configuration["SqlServer:FirstConnectionString"],
            System.StringComparison.Ordinal
        );
        Assert.EndsWith(
            expectedSecondPartOfConnectionString2,
            configuration["SqlServer:SecondConnectionString"],
            System.StringComparison.Ordinal
        );
    }

    [Fact]
    public void ConfigurationOverrides_AdConnectionString_and_Add_methods_test()
    {
        var connectionStringKey = "ConnectionStrings:DbContext";
        var dbPrefix = "prefix";

        var expectedDbName = $"{dbPrefix}_";
        var expectedFirstPartOfConnectionString = $"Database={expectedDbName}";
        var expectedSecondPartOfConnectionString = $"Server=(local);Database=DbContext;Trusted_Connection=True;";

        Environment.SetEnvironmentVariable(
            connectionStringKey,
            "Server=(local);Database=DbContext;Trusted_Connection=True;"
        );

        var overrides = new ConfigurationOverrides()
            .AddConnectionString("SqlServer:ConnectionString", dbPrefix, connectionStringKey)
            .Add("key", "value");

        var configuration = new ConfigurationBuilder().Add(overrides).Build();

        Assert.Equal("value", configuration["key"]);

        Assert.NotNull(configuration["SqlServer:ConnectionString"]);

        Assert.StartsWith(
            expectedFirstPartOfConnectionString,
            configuration["SqlServer:ConnectionString"],
            System.StringComparison.Ordinal
        );

        Assert.EndsWith(
            expectedSecondPartOfConnectionString,
            configuration["SqlServer:ConnectionString"],
            System.StringComparison.Ordinal
        );
    }
}
