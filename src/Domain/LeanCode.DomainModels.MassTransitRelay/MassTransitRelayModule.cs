using System;
using System.Collections.Generic;
using Autofac;
using GreenPipes;
using LeanCode.Components;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public delegate IBusControl BusFactory(IComponentContext context, string queueName);

    public class MassTransitRelayModule : AppModule
    {
        private readonly string queueName;
        private readonly TypesCatalog consumers;
        private readonly BusFactory busFactory;

        public MassTransitRelayModule(
            string queueName,
            TypesCatalog consumers,
            BusFactory? busFactory = null)
        {
            this.queueName = queueName;
            this.consumers = consumers;
            this.busFactory = busFactory ?? DefaultBusFactory;
        }

        public override void ConfigureServices(IServiceCollection services) =>
            services.AddHostedService<MassTransitRelayHostedService>();

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(EventsPublisherElement<,,>))
                .AsSelf();

            builder.RegisterType<BusEventPublisher>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.AddMassTransit(cfg =>
            {
                cfg.AddConsumers(consumers.Assemblies);
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
                var scopeFactory = context.Resolve<Func<ILifetimeScope>>();

                config.UseLogsCorrelation();
                config.UseRetry(retryConfig =>
                    retryConfig.Incremental(5, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)));

                config.UseInMemoryOutbox();

                config.UseConsumedMessagesFiltering();
                config.UseDomainEventsPublishing();
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
