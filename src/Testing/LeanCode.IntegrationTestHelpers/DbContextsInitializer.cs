using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Polly;

namespace LeanCode.IntegrationTestHelpers
{
    public class DbContextsInitializer : IHostedService
    {
        private static readonly IAsyncPolicy CreatePolicy = Policy
            .Handle<SqlException>(e => e.Number == 5177)
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(0.5),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
            });

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<DbContextsInitializer>();

        private readonly Func<IEnumerable<DbContext>> getContexts;

        public DbContextsInitializer(Func<IEnumerable<DbContext>> getContexts)
        {
            this.getContexts = getContexts;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var ctx in getContexts())
            {
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
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var ctx in getContexts())
            {
                logger.Information("Dropping database for context {ContextType}", ctx.GetType());
                await ctx.Database.EnsureDeletedAsync(cancellationToken);
            }
        }
    }
}
