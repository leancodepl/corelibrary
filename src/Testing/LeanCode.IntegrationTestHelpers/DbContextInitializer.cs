using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.Retry;

namespace LeanCode.IntegrationTestHelpers;

public class DbContextInitializer<T>(ILogger<DbContextInitializer<T>> logger, IServiceProvider serviceProvider)
    : IHostedService
    where T : DbContext
{
    private static readonly AsyncRetryPolicy CreatePolicy = Policy
        .Handle<SqlException>(e => e.Number == 5177)
        .Or<NpgsqlException>(e => e.IsTransient)
        .WaitAndRetryAsync([TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3)]);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<T>();
        logger.LogInformation("Creating database for context {ContextType}", context.GetType());
        // HACK: should mitigate (slightly) the bug in MSSQL that prevents us from creating
        // new databases.
        // See https://github.com/Microsoft/mssql-docker/issues/344 for tracking issue.
        await CreatePolicy.ExecuteAsync(
            async token =>
            {
                await context.Database.EnsureDeletedAsync(token);
                await context.Database.EnsureCreatedAsync(token);

                if (context.Database.GetDbConnection() is NpgsqlConnection connection)
                {
                    if (connection.State == System.Data.ConnectionState.Closed)
                    {
                        await connection.OpenAsync(token);
                        await connection.ReloadTypesAsync(token);
                        await connection.CloseAsync();
                    }
                    else
                    {
                        await connection.ReloadTypesAsync(token);
                    }
                }
            },
            cancellationToken
        );
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<T>();
        logger.LogInformation("Dropping database for context {ContextType}", context.GetType());
        await context.Database.EnsureDeletedAsync(CancellationToken.None);
    }
}
