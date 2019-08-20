using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public class MassTransitRelayHostedService : IHostedService
    {
        private readonly IBusControl bus;

        public MassTransitRelayHostedService(IBusControl bus)
        {
            this.bus = bus;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return bus.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return bus.StopAsync();
        }
    }
}
