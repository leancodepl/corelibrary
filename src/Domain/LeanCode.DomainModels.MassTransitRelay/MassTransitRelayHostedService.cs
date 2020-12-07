using System.Threading;
using System.Threading.Tasks;
using LeanCode.OrderedHostedServices;
using MassTransit;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public class MassTransitRelayHostedService : IOrderedHostedService
    {
        private readonly IBusControl bus;

        public int Order => int.MaxValue;

        public MassTransitRelayHostedService(IBusControl bus)
        {
            this.bus = bus;
        }

        public Task StartAsync(CancellationToken cancellationToken) => bus.StartAsync(cancellationToken);
        public Task StopAsync(CancellationToken cancellationToken) => bus.StopAsync(cancellationToken);
    }
}
