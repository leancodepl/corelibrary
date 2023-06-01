using LeanCode.DomainModels.MassTransitRelay.Testing;
using MassTransit;
using MassTransit.Testing.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.DomainModels.MassTransitRelay;

public static class MassTransitRelayServiceCollectionExtensions
{
    public static IServiceCollection AddCQRSMassTransitIntegration(
        this IServiceCollection services,
        Action<IBusRegistrationConfigurator> busCfg,
        Action<MassTransitHostOptions>? hostCfg = null
    )
    {
        services.AddAsyncEventsInterceptor();

        hostCfg ??= opts =>
        {
            opts.WaitUntilStarted = true;
        };

        services.AddOptions<MassTransitHostOptions>().Configure(hostCfg);
        services.AddMassTransit(busCfg);

        return services;
    }

    public static IServiceCollection AddAsyncEventsInterceptor(this IServiceCollection services)
    {
        var interceptor = new AsyncEventsInterceptor();
        interceptor.Configure();
        services.AddSingleton(interceptor);

        return services;
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
