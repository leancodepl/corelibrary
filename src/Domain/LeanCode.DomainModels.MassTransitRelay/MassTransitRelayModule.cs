using System;
using System.Collections.Generic;
using Autofac;
using GreenPipes;
using LeanCode.Components;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public delegate void BusConfig(IComponentContext context, IBusFactoryConfigurator config, string queueName);

    public abstract class MassTransitRelayModuleBase : AppModule
    {
        private readonly string queueName;
        private readonly TypesCatalog consumers;
        private readonly BusConfig busConfig;

        protected abstract IBusControl CreateBus(Action<IBusFactoryConfigurator> configurator);

        public MassTransitRelayModuleBase(
            string queueName,
            TypesCatalog consumers,
            BusConfig? busConfig = null)
        {
            this.queueName = queueName;
            this.consumers = consumers;
            this.busConfig = busConfig ?? DefaultBusConfig;
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
            return CreateBus(cfg => busConfig(context, cfg, queueName));
        }

        public static void DefaultBusConfig(
            IComponentContext context,
            IBusFactoryConfigurator config,
            string queueName)
        {
            var scopeFactory = context.Resolve<Func<ILifetimeScope>>();

            config.UseLogsCorrelation();
            config.UseRetry(retryConfig =>
                retryConfig.Incremental(5, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)));

            config.UseInMemoryOutbox();
            config.UseLifetimeScopeInjection(scopeFactory);
            config.UseDomainEventsPublishing();
            config.ReceiveEndpoint(queueName, rcv =>
            {
                rcv.ConfigureConsumers(context);

                var recvObservers = context.Resolve<IEnumerable<IReceiveEndpointObserver>>();
                foreach (var obs in recvObservers)
                {
                    rcv.ConnectReceiveEndpointObserver(obs);
                }
            });

            var observers = context.Resolve<IEnumerable<IBusObserver>>();
            foreach (var obs in observers)
            {
                config.ConnectBusObserver(obs);
            }
        }
    }

    public class MassTransitInMemoryRelayModule : MassTransitRelayModuleBase
    {
        public MassTransitInMemoryRelayModule(string queueName, TypesCatalog consumersAssemblies)
            : base(queueName, consumersAssemblies) { }

        protected override IBusControl CreateBus(Action<IBusFactoryConfigurator> configurator) =>
            Bus.Factory.CreateUsingInMemory(configurator);
    }
}
