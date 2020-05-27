using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.AsyncInitializer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace LeanCode.IntegrationTestHelpers
{
    public class DbContextsInitializer : IAsyncInitializable
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

        public int Order => int.MinValue;

        public DbContextsInitializer(Func<IEnumerable<DbContext>> getContexts)
        {
            this.getContexts = getContexts;
        }

        public async Task InitializeAsync()
        {
            foreach (var ctx in getContexts())
            {
                logger.Information("Creating database for context {ContextType}", ctx.GetType());
                // HACK: should mitigate (slightly) the bug in MSSQL that prevents us from creating
                // new databases.
                // See https://github.com/Microsoft/mssql-docker/issues/344 for tracking issue.
                await CreatePolicy.ExecuteAsync(async () =>
                {
                    await ctx.Database.EnsureDeletedAsync();
                    await ctx.Database.EnsureCreatedAsync();
                });
            }
        }

        public async Task DeinitializeAsync()
        {
            foreach (var ctx in getContexts())
            {
                logger.Information("Dropping database for context {ContextType}", ctx.GetType());
                await ctx.Database.EnsureDeletedAsync();
            }
        }
    }
}
