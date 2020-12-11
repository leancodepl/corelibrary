using Microsoft.Extensions.Hosting;

namespace LeanCode.OrderedHostedServices
{
    public abstract class OrderedBackgroundService : BackgroundService, IOrderedHostedService
    {
        public abstract int Order { get; }
    }
}
