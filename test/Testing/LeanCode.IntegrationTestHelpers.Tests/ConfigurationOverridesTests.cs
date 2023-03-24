using Microsoft.Extensions.Configuration;
using Xunit;

namespace LeanCode.IntegrationTestHelpers.Tests;

public class ConfigurationOverridesTests
{
    [Fact]
    public void TestBuild()
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

        var overrides = ConfigurationOverrides
            .CreateBuilder()
            .AddConnectionString("SqlServer:FirstConnectionString", dbPrefix1, connectionStringKey1)
            .AddConnectionString("SqlServer:SecondConnectionString", dbPrefix2, connectionStringKey2)
            .Build();

        var configuration = new ConfigurationBuilder().Add(overrides).Build();

        Assert.NotNull(configuration["SqlServer:FirstConnectionString"]);
        Assert.NotNull(configuration["SqlServer:SecondConnectionString"]);

        Assert.StartsWith(expectedFirstPartOfConnectionString1, configuration["SqlServer:FirstConnectionString"]);
        Assert.StartsWith(expectedFirstPartOfConnectionString2, configuration["SqlServer:SecondConnectionString"]);

        Assert.EndsWith(expectedSecondPartOfConnectionString1, configuration["SqlServer:FirstConnectionString"]);
        Assert.EndsWith(expectedSecondPartOfConnectionString2, configuration["SqlServer:SecondConnectionString"]);
    }
}
