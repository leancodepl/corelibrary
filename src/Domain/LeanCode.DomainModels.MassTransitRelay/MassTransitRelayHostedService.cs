using System.Threading.Tasks;
using LeanCode.AsyncInitializer;
using MassTransit;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public class MassTransitRelayHostedService : IAsyncInitializable
    {
        private readonly IBusControl bus;

        public int Order => int.MaxValue;

        public MassTransitRelayHostedService(IBusControl bus)
        {
            this.bus = bus;
        }

        public Task InitializeAsync() => bus.StartAsync();
        public Task DeinitializeAsync() => bus.StopAsync();
    }
}
