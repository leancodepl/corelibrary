using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using NSubstitute;

namespace LeanCode.AsyncInitializer.Tests
{
    internal class CountedServer : IServer
    {
        private readonly Counter counter;

        public int? StartOrder { get; set; }
        public int? StopOrder { get; set; }
        public int? DisposeOrder { get; set; }

        public IFeatureCollection Features { get; } = Substitute.For<IFeatureCollection>();

        public CountedServer(Counter counter)
        {
            this.counter = counter;
        }

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            StartOrder = counter.Next();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            StopOrder = counter.Next();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            DisposeOrder = counter.Next();
        }
    }
}
