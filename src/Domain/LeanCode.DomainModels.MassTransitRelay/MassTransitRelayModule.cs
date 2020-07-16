using System;
using System.Reflection;
using Autofac;
using GreenPipes;
using LeanCode.Components;
using LeanCode.DomainModels.MassTransitRelay.Inbox;
using LeanCode.DomainModels.MassTransitRelay.Middleware;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using LeanCode.PeriodicService;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public delegate IBusControl BusFactory(IComponentContext context);

    public class MassTransitRelayModule : AppModule
    {
        private readonly TypesCatalog consumersCatalog;
        private readonly TypesCatalog eventsCatalog;
        private readonly BusFactory busFactory;

        public MassTransitRelayModule(
            TypesCatalog consumersCatalog,
            TypesCatalog eventsCatalog,
            BusFactory? busFactory = null)
        {
            this.consumersCatalog = consumersCatalog;
            this.eventsCatalog = eventsCatalog;
            this.busFactory = busFactory ?? DefaultBusFactory;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<MassTransitRelayHostedService>();
            services.AddHostedService<PeriodicHostedService<ConsumedMessagesCleaner>>();
            services.AddHostedService<PeriodicHostedService<PeriodicEventsPublisher>>();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(EventsPublisherElement<,,>)).AsSelf();
            builder.RegisterGeneric(typeof(StoreAndPublishEventsElement<,,>)).AsSelf();
            builder.RegisterType<EventPublisher>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<EventsStore>().AsSelf();
            builder.RegisterType<ConsumedMessagesCleaner>().AsSelf();
            builder.RegisterType<PeriodicEventsPublisher>().AsSelf();

            builder.RegisterInstance(new JsonEventsSerializer(eventsCatalog))
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.AddMassTransit(cfg =>
            {
                cfg.AddConsumers(consumersCatalog.Assemblies);
                cfg.AddBus(CreateBus);
            });
        }

        private IBusControl CreateBus(IComponentContext context)
        {
            return busFactory(context);
        }

        public static IBusControl DefaultBusFactory(IComponentContext context)
        {
            return Bus.Factory.CreateUsingInMemory(config =>
            {
                var queueName = Assembly.GetEntryAssembly()!.GetName().Name;

                config.UseLogsCorrelation();
                config.UseRetry(retryConfig =>
                    retryConfig.Incremental(5, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)));

                config.UseConsumedMessagesFiltering();
                config.StoreAndPublishDomainEvents();
                config.ReceiveEndpoint(queueName, rcv =>
                {
                    rcv.ConfigureConsumers(context);
                    rcv.ConnectReceiveEndpointObservers(context);
                });

                config.ConnectBusObservers(context);
            });
        }
    }
}
