using System.Runtime.CompilerServices;
using FluentAssertions;
using LeanCode.Npgsql.ActiveDirectory;
using LeanCode.Test.Helpers;
using Npgsql;

namespace LeanCode.Azure.Tests.PostgresAD;

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

public sealed class PostgresFactAttribute(
    [CallerFilePath] string? sourceFilePath = null,
    [CallerLineNumber] int sourceLineNumber = 0
) : ExternalServiceFactAttribute(sourceFilePath, sourceLineNumber)
{
    protected override string ServiceType => "npgsql";
    protected override IReadOnlyCollection<string> RequiredEnvVariables { get; } =
    [Env.TenantIdKey, Env.ClientIdKey, Env.ClientSecretKey, Env.NpgsqlConnectionStringKey];
}
