using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.AsyncInitializer
{
    public sealed class AsyncInitializer : IServer
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<AsyncInitializer>();

        private readonly IServiceProvider provider;
        private readonly IServer innerServer;

        public IFeatureCollection Features => innerServer.Features;

        public AsyncInitializer(IServiceProvider provider, IServer innerServer)
        {
            this.provider = provider;
            this.innerServer = innerServer;
        }

        public async Task StartAsync<TContext>(
            IHttpApplication<TContext> application,
            CancellationToken cancellationToken)
        {
            await InitializeAsync(cancellationToken);
            await innerServer.StartAsync(application, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await innerServer.StopAsync(cancellationToken);
            await DisposeAsync(cancellationToken);
        }

        public void Dispose()
        {
            innerServer.Dispose();
        }

        private async Task InitializeAsync(CancellationToken token)
        {
            logger.Information("Initializing async modules");
            var scopeFactory = provider.GetService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var inits = scope.ServiceProvider
                    .GetService<IEnumerable<IAsyncInitializable>>()
                    .OrderBy(i => i.Order);

                foreach (var i in inits)
                {
                    if (token.IsCancellationRequested)
                    {
                        logger.Information("Cancellation requested, skipping initialization");
                        break;
                    }

                    logger.Debug("Initializing {Type}", i.GetType());
                    await i.InitializeAsync();
                }
            }
        }

        private async Task DisposeAsync(CancellationToken token)
        {
            logger.Information("Disposing async modules");
            var scopeFactory = provider.GetService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var inits = scope.ServiceProvider
                    .GetService<IEnumerable<IAsyncInitializable>>()
                    .OrderByDescending(i => i.Order);

                foreach (var i in inits)
                {
                    if (token.IsCancellationRequested)
                    {
                        logger.Warning("Cancellation required, skipping rest of the disposals");
                        break;
                    }

                    logger.Debug("Disposing {Type}", i.GetType());
                    await i.DisposeAsync();
                }
            }
        }
    }
}
