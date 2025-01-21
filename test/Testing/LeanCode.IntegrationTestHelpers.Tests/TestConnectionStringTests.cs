using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LeanCode.IntegrationTestHelpers.Tests;

public class TestConnectionStringTests
{
    private const string ConnectionStringKey = "ConnectionString";
    private const string BaseKey = "Base";

    [Fact]
    public void Generates_connection_string()
    {
        var config = Build("");

        config[ConnectionStringKey].Should().NotBeNull();
    }

    [Fact]
    public void Preserves_base_connection_string()
    {
        var config = Build("");

        config[BaseKey].Should().NotBeNull();
    }

    [Fact]
    public void Final_connection_string_contains_Database_parameter()
    {
        var config = Build("");

        config[ConnectionStringKey].Should().Contain("Database=");
    }

    [Fact]
    public void Final_connection_string_contains_base_connection_string()
    {
        const string BaseConnectionString = "Some=1;Parameter=2";
        var config = Build(BaseConnectionString);

        config[ConnectionStringKey].Should().Contain(BaseConnectionString);
    }

    private static IConfiguration Build(string baseConnectionString)
    {
        var builder = new ConfigurationBuilder();
        builder.Add(new ConfigurationOverrides(new() { [BaseKey] = baseConnectionString }));
        builder.Add(new TestConnectionString(BaseKey, ConnectionStringKey));
        return builder.Build();
    }
}
