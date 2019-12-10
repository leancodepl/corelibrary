using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.AsyncInitializer;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.IntegrationTestHelpers
{
    public class DbContextsInitializer : IAsyncInitializable
    {
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
                await ctx.Database.EnsureCreatedAsync();
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
