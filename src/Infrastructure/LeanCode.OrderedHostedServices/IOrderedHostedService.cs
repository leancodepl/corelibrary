using Microsoft.Extensions.Hosting;

namespace LeanCode.OrderedHostedServices
{
    public interface IOrderedHostedService : IHostedService
    {
        int Order { get; }
    }
}
