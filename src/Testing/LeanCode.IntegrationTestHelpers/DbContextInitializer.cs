using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Polly;

namespace LeanCode.IntegrationTestHelpers;

public class DbContextInitializer<T> : IHostedService
    where T : DbContext
{
    private static readonly IAsyncPolicy CreatePolicy = Policy
        .Handle<SqlException>(e => e.Number == 5177)
        .WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), });

    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<DbContextInitializer<T>>();

    private readonly T context;

    public DbContextInitializer(T context)
    {
        this.context = context;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.Information("Creating database for context {ContextType}", context.GetType());
        // HACK: should mitigate (slightly) the bug in MSSQL that prevents us from creating
        // new databases.
        // See https://github.com/Microsoft/mssql-docker/issues/344 for tracking issue.
        await CreatePolicy.ExecuteAsync(
            async token =>
            {
                await context.Database.EnsureDeletedAsync(token);
                await context.Database.EnsureCreatedAsync(token);
            },
            cancellationToken
        );
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.Information("Dropping database for context {ContextType}", context.GetType());
        await context.Database.EnsureDeletedAsync(cancellationToken);
    }
}
