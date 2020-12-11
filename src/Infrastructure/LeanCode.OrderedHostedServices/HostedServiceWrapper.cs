using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace LeanCode.OrderedHostedServices
{
    public class HostedServiceWrapper<T> : IOrderedHostedService
        where T : IHostedService
    {
        private readonly IHostedService service;

        public int Order { get; }

        public HostedServiceWrapper(T service, int order)
        {
            this.service = service;
            Order = order;
        }

        Task IHostedService.StartAsync(CancellationToken token) => service.StartAsync(token);
        Task IHostedService.StopAsync(CancellationToken token) => service.StopAsync(token);
    }
}
