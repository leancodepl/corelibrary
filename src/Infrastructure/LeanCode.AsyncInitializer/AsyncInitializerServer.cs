using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

namespace LeanCode.AsyncInitializer
{
    public class AsyncInitializerServer : IServer
    {
        private readonly AsyncInitializer initializer;
        private readonly IServer innerServer;

        public IFeatureCollection Features => innerServer.Features;

        public AsyncInitializerServer(AsyncInitializer initializer, IServer innerServer)
        {
            this.initializer = initializer;
            this.innerServer = innerServer;
        }

        public async Task StartAsync<TContext>(
            IHttpApplication<TContext> application,
            CancellationToken cancellationToken)
        {
            await initializer.InitializeAsync(cancellationToken);
            await innerServer.StartAsync(application, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await innerServer.StopAsync(cancellationToken);
            await initializer.DeinitializeAsync(cancellationToken);
        }

        public void Dispose() => innerServer.Dispose();
    }
}
