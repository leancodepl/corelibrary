using Autofac;
using LeanCode.AzureIdentity;
using LeanCode.Components;
using LeanCode.DomainModels.MassTransitRelay;
using LeanCode.DomainModels.MassTransitRelay.Middleware;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using ExampleApp.Core.Services.DataAccess.Serialization;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ExampleApp.Api;

public class ExampleAppMassTransitModule : MassTransitRelayModule
{
    private const string QueueName = "exampleapp-events";

    private readonly TypesCatalog consumersCatalog;
    private readonly TypesCatalog eventsCatalog;
    private readonly IConfiguration config;
    private readonly IWebHostEnvironment hostEnv;

    public ExampleAppMassTransitModule(
        TypesCatalog consumersCatalog,
        TypesCatalog eventsCatalog,
        IConfiguration config,
        IWebHostEnvironment hostEnv)
        : base(eventsCatalog)
    {
        this.consumersCatalog = consumersCatalog;
        this.eventsCatalog = eventsCatalog;
        this.config = config;
        this.hostEnv = hostEnv;
    }

    public override void ConfigureMassTransit(IServiceCollection services)
    {
        services.AddMassTransit(cfg =>
        {
            cfg.AddConsumers(consumersCatalog.Assemblies.ToArray());

            if (hostEnv.IsDevelopment())
            {
                cfg.AddDelayedMessageScheduler();

                cfg.UsingInMemory(
                    (ctx, cfg) =>
                    {
                        cfg.UseDelayedMessageScheduler();
                        ConfigureBusCommon(ctx, cfg);
                    });
            }
            else
            {
                var endpoint = Config.MassTransit.AzureServiceBus.Endpoint(config);

                cfg.AddServiceBusMessageScheduler();
                cfg.UsingAzureServiceBus((ctx, cfg) =>
                {
                    cfg.UseDelayedMessageScheduler();

                    cfg.Host(new Uri(endpoint), host =>
                    {
                        host.RetryLimit = 5;
                        host.RetryMinBackoff = TimeSpan.FromSeconds(3);
                        host.TokenCredential = DefaultLeanCodeCredential.Create(config);
                    });

                    ConfigureBusCommon(ctx, cfg);
                });
            }
        });
    }

    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder
            .RegisterInstance(new SystemTextJsonEventsSerializer(eventsCatalog, KnownConverters.AddAll(new())))
            .AsImplementedInterfaces()
            .SingleInstance();
    }

    private static void ConfigureBusCommon(IBusRegistrationContext ctx, IBusFactoryConfigurator cfg)
    {
        cfg.ConfigureJsonSerializerOptions(KnownConverters.AddAll);

        cfg.ReceiveEndpoint(
            QueueName,
            rcv =>
            {
                rcv.UseLogsCorrelation();
                rcv.UseRetry(retryConfig =>
                    retryConfig.Incremental(5, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)));
                rcv.UseConsumedMessagesFiltering(ctx);
                rcv.StoreAndPublishDomainEvents(ctx);

                rcv.ConfigureConsumers(ctx);
                rcv.ConnectReceiveEndpointObservers(ctx);
            });

        cfg.ConnectBusObservers(ctx);
    }
}
