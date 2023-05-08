using Autofac;
using LeanCode.Components;
using LeanCode.DomainModels.MassTransitRelay.Middleware;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.MassTransitRelay;

public abstract class MassTransitRelayModule : AppModule
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterGeneric(typeof(EventsPublisherElement<,,>)).AsSelf();
        builder.RegisterGeneric(typeof(CommitDatabaseTransactionElement<,,,>)).AsSelf();
        builder
            .RegisterType<AsyncEventsInterceptor>()
            .AsSelf()
            .OnActivated(a => a.Instance.Configure())
            .SingleInstance();
    }

    public sealed override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddOptions<MassTransitHostOptions>()
            .Configure(opts =>
            {
                opts.WaitUntilStarted = true;
            });

        ConfigureMassTransit(services);
    }

    public abstract void ConfigureMassTransit(IServiceCollection services);
}
