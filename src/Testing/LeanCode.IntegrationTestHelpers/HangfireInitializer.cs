using System.Threading;
using System.Threading.Tasks;
using LeanCode.Dapper;
using LeanCode.OrderedHostedServices;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.IntegrationTestHelpers
{
    public sealed class HangfireInitializer<TContext> : IOrderedHostedService
        where TContext : DbContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<HangfireInitializer<TContext>>();

        private readonly TContext dbContext;

        public int Order => int.MinValue + 1;

        public HangfireInitializer(TContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.Information("Installing Hangfire");

            var installScript = Hangfire.SqlServer.SqlServerObjectsInstaller.GetInstallScript(null, false);
            await dbContext.ExecuteAsync(installScript, cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
