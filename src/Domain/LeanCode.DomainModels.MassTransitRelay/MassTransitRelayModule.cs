using System;
using Autofac;
using GreenPipes;
using LeanCode.Components;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public abstract class MassTransitRelayModuleBase : AppModule
    {
        protected TypesCatalog Consumers { get; }

        public MassTransitRelayModuleBase(TypesCatalog consumers)
        {
            Consumers = consumers;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<MassTransitRelayHostedService>();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(EventsPublisherElement<,,>))
                .AsSelf();

            builder.AddMassTransit(cfg =>
            {
                cfg.AddConsumers(Consumers.Assemblies);
                cfg.AddBus(CreateBus);
            });
        }

        protected abstract IBusControl CreateBus(IComponentContext context);

        protected void ConfigureCommonFilters(IBusFactoryConfigurator bus, IComponentContext context)
        {
            var scopeFactory = context.Resolve<Func<ILifetimeScope>>();

            bus.UseSerilog();
            bus.UseRetry(retryConfig => retryConfig.Incremental(5, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10)));
            bus.UseLifetimeScopeInjection(scopeFactory);
            bus.UseDomainEventsPublishing();
        }

        public static void DefaultReceiveEndpointConfig(IInMemoryReceiveEndpointConfigurator config, IComponentContext context)
        {
            config.ConfigureConsumers(context);
        }
    }

    public class MassTransitInMemoryRelayModule : MassTransitRelayModuleBase
    {
        private readonly string queueName;
        private readonly Action<IInMemoryReceiveEndpointConfigurator, IComponentContext> receiveConfig;

        public MassTransitInMemoryRelayModule(
            string queueName,
            TypesCatalog consumersAssemblies,
            Action<IInMemoryReceiveEndpointConfigurator, IComponentContext> receiveConfig = null)
        : base(consumersAssemblies)
        {
            this.queueName = queueName;
            this.receiveConfig = receiveConfig ?? DefaultReceiveEndpointConfig;
        }

        protected override IBusControl CreateBus(IComponentContext context)
        {
            return Bus.Factory.CreateUsingInMemory(cfg =>
            {
                ConfigureCommonFilters(cfg, context);
                cfg.ReceiveEndpoint(queueName, rcv => receiveConfig(rcv, context));
            });
        }
    }
}
