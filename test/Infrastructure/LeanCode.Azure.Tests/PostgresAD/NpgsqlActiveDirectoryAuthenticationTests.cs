using FluentAssertions;
using LeanCode.Npgsql.ActiveDirectory;
using Npgsql;
using Xunit;

namespace LeanCode.Azure.Tests.PostgresAD;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        Skip = Env.SkipIfVariablesNotSet(
            Env.TenantIdKey,
            Env.ClientIdKey,
            Env.ClientSecretKey,
            Env.NpgsqlConnectionStringKey
        );
    }
}

public class NpgsqlActiveDirectoryAuthenticationTests
{
    [PostgresFact]
    public async Task Authentication_works()
    {
        var connString = Environment.GetEnvironmentVariable(Env.NpgsqlConnectionStringKey);
        var credential = Env.GetTokenCredential();

        var dataSource = new NpgsqlDataSourceBuilder(connString)
            .UseAzureActiveDirectoryAuthentication(credential)
            .Build();

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1";
        var result = await command.ExecuteScalarAsync();
        result.Should().Be(1);
    }
}
