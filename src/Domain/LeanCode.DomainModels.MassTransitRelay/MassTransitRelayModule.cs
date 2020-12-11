using System;
using System.Reflection;
using Autofac;
using GreenPipes;
using LeanCode.Components;
using LeanCode.DomainModels.MassTransitRelay.Inbox;
using LeanCode.DomainModels.MassTransitRelay.Middleware;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using LeanCode.DomainModels.MassTransitRelay.Simple;
using LeanCode.PeriodicService;
using MassTransit;
using MassTransit.AutofacIntegration;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public class MassTransitRelayModule : AppModule
    {
        private readonly TypesCatalog consumersCatalog;
        private readonly TypesCatalog eventsCatalog;
        private readonly Action<IContainerBuilderBusConfigurator> busConfig;
        private readonly bool useInbox;
        private readonly bool useOutbox;

        public MassTransitRelayModule(
            TypesCatalog consumersCatalog,
            TypesCatalog eventsCatalog,
            Action<IContainerBuilderBusConfigurator>? busConfig = null,
            bool useInbox = true,
            bool useOutbox = true)
        {
            this.consumersCatalog = consumersCatalog;
            this.eventsCatalog = eventsCatalog;
            this.busConfig = busConfig ?? DefaultBusConfigurator;
            this.useInbox = useInbox;
            this.useOutbox = useOutbox;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(EventsPublisherElement<,,>)).AsSelf();
            builder.RegisterGeneric(typeof(StoreAndPublishEventsElement<,,>)).AsSelf();
            builder.RegisterType<EventPublisher>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<EventsStore>().AsSelf();

            if (useInbox)
            {
                builder.RegisterType<ConsumedMessagesCleaner>().AsSelf();
                builder.RegisterPeriodicAction<ConsumedMessagesCleaner>();
            }

            if (useOutbox)
            {
                builder.RegisterType<PeriodicEventsPublisher>().AsSelf();
                builder.RegisterType<PublishedEventsCleaner>().AsSelf();
                builder.RegisterPeriodicAction<PeriodicEventsPublisher>();
                builder.RegisterPeriodicAction<PublishedEventsCleaner>();
            }

            builder.RegisterInstance(new JsonEventsSerializer(eventsCatalog))
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<AsyncEventsInterceptor>()
                .AsSelf()
                .OnActivated(a => a.Instance.Configure())
                .SingleInstance();

            builder.RegisterType<SimpleEventsExecutor>()
                .AsSelf()
                .SingleInstance()
                .WithParameter("useOutbox", useOutbox);

            builder.RegisterType<SimpleFinalizer>().AsSelf();

            builder.AddMassTransit(cfg =>
            {
                cfg.AddConsumers(consumersCatalog.Assemblies);
                busConfig(cfg);
            });

            builder.RegisterType<MassTransitRelayHostedService>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        public static void DefaultBusConfigurator(IContainerBuilderBusConfigurator busCfg)
        {
            busCfg.UsingInMemory((context, config) =>
            {
                var queueName = Assembly.GetEntryAssembly()!.GetName().Name;

                config.ReceiveEndpoint(queueName, rcv =>
                {
                    rcv.UseLogsCorrelation();
                    rcv.UseRetry(retryConfig =>
                        retryConfig.Incremental(5, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)));
                    rcv.UseConsumedMessagesFiltering();
                    rcv.StoreAndPublishDomainEvents();

                    rcv.ConfigureConsumers(context);
                    rcv.ConnectReceiveEndpointObservers(context);
                });

                config.ConnectBusObservers(context);
            });
        }
    }
}
