using System;
using Autofac;
using GreenPipes;
using LeanCode.Components;
using LeanCode.DomainModels.MassTransitRelay.Inbox;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using LeanCode.PeriodicService;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public delegate IBusControl BusFactory(IComponentContext context, string queueName);

    public class MassTransitRelayModule : AppModule
    {
        private readonly string queueName;
        private readonly TypesCatalog consumersCatalog;
        private readonly TypesCatalog eventsCatalog;
        private readonly BusFactory busFactory;

        public MassTransitRelayModule(
            string queueName,
            TypesCatalog consumersCatalog,
            TypesCatalog eventsCatalog,
            BusFactory? busFactory = null)
        {
            this.queueName = queueName;
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
            builder.RegisterGeneric(typeof(EventsPublisherElement<,,>))
                .AsSelf();

            builder.RegisterGeneric(typeof(StoreAndPublishEventsElement<,,>))
                .AsSelf();

            builder.RegisterType<EventPublisher>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<StoreAndPublishEventsImpl>()
                .AsSelf();

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
            return busFactory(context, queueName);
        }

        public static IBusControl DefaultBusFactory(
            IComponentContext context,
            string queueName)
        {
            return Bus.Factory.CreateUsingInMemory(config =>
            {
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
