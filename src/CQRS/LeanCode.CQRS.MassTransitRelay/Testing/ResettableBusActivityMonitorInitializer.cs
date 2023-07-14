using Microsoft.Extensions.Hosting;

namespace LeanCode.CQRS.MassTransitRelay.Testing;

internal sealed class ResettableBusActivityMonitorInitializer : IHostedService
{
    public ResettableBusActivityMonitorInitializer(ResettableBusActivityMonitor monitor)
    {
        // ServiceCollection does not allow any auto-initialize of services,
        // so we just want to instantiate it somehow, to start observing the bus
        _ = monitor;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
