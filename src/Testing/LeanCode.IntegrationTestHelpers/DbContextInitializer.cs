using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Polly;

namespace LeanCode.IntegrationTestHelpers
{
    public class DbContextInitializer<T> : IHostedService
        where T : DbContext
    {
        private static readonly IAsyncPolicy CreatePolicy = Policy
            .Handle<SqlException>(e => e.Number == 5177)
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(0.5),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
            });

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<DbContextInitializer<T>>();

        private readonly Func<T> getContext;

        public DbContextInitializer(Func<T> getContext)
        {
            this.getContext = getContext;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var ctx = getContext();
            logger.Information("Creating database for context {ContextType}", ctx.GetType());
            // HACK: should mitigate (slightly) the bug in MSSQL that prevents us from creating
            // new databases.
            // See https://github.com/Microsoft/mssql-docker/issues/344 for tracking issue.
            await CreatePolicy.ExecuteAsync(
                async token =>
                {
                    await ctx.Database.EnsureDeletedAsync(token);
                    await ctx.Database.EnsureCreatedAsync(token);
                },
                cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await using var ctx = getContext();
            logger.Information("Dropping database for context {ContextType}", ctx.GetType());
            await ctx.Database.EnsureDeletedAsync(cancellationToken);
        }
    }
}
