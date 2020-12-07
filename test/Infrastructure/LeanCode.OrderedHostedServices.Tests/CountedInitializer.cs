using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace LeanCode.OrderedHostedServices.Tests
{
    internal class Counter
    {
        private int i = -1;

        public int Next() => Interlocked.Increment(ref i);
    }

    internal class CountedInitializer : IOrderedHostedService
    {
        private readonly int order;
        private readonly Counter counter;

        public CountedInitializer(int order, Counter counter)
        {
            this.order = order;
            this.counter = counter;
        }

        public int? StartOrder { get; private set; }
        public int? StopOrder { get; private set; }

        int IOrderedHostedService.Order => order;

        Task IHostedService.StartAsync(CancellationToken token)
        {
            StartOrder = counter.Next();
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken token)
        {
            StopOrder = counter.Next();
            return Task.CompletedTask;
        }
    }
}
