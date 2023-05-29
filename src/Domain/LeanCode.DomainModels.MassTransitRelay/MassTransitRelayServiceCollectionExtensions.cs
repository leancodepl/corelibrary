using LeanCode.DomainModels.MassTransitRelay.Testing;
using MassTransit;
using MassTransit.Testing.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.MassTransitRelay;

public static class MassTransitRelayServiceCollectionExtensions
{
    public static void AddCQRSMassTransitIntegration(
        this IServiceCollection services,
        Action<MassTransitHostOptions>? hostCfg = null
    )
    {
        var interceptor = new AsyncEventsInterceptor();
        interceptor.Configure();
        services.AddSingleton(interceptor);

        hostCfg ??= opts =>
        {
            opts.WaitUntilStarted = true;
        };

        services.AddOptions<MassTransitHostOptions>().Configure(hostCfg);
    }

    public static void AddBusActivityMonitor(this IServiceCollection services, TimeSpan? inactivityWaitTime = null)
    {
        services.AddSingleton(
            sp =>
                ResettableBusActivityMonitor.CreateFor(
                    sp.GetRequiredService<IBusControl>(),
                    inactivityWaitTime ?? TimeSpan.FromSeconds(1)
                )
        );
        services.AddSingleton<IBusActivityMonitor>(sp => sp.GetRequiredService<ResettableBusActivityMonitor>());
    }
}
