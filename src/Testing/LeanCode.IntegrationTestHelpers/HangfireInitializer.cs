using System.Threading;
using System.Threading.Tasks;
using LeanCode.Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace LeanCode.IntegrationTestHelpers
{
    public sealed class HangfireInitializer<TContext> : IHostedService
        where TContext : DbContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<HangfireInitializer<TContext>>();

        private readonly TContext dbContext;

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
