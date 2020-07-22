using System.Threading.Tasks;
using LeanCode.AsyncInitializer;
using LeanCode.Dapper;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.IntegrationTestHelpers
{
    public sealed class HangfireInitializer<TContext> : IAsyncInitializable
        where TContext : DbContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<HangfireInitializer<TContext>>();

        private readonly TContext dbContext;

        public int Order => int.MinValue + 1;

        public HangfireInitializer(TContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task DeinitializeAsync() => Task.CompletedTask;

        public async Task InitializeAsync()
        {
            logger.Information("Installing Hangfire");

            await dbContext.WithConnectionAsync(c =>
            {
                Hangfire.SqlServer.SqlServerObjectsInstaller.Install(c);
                return Task.FromResult(false);
            });
        }
    }
}
