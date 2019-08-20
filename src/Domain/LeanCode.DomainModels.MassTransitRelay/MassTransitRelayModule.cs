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
    }

    public class MassTransitInMemoryRelayModule : MassTransitRelayModuleBase
    {
        private readonly string queueName;
        private readonly Action<IReceiveEndpointConfigurator, IComponentContext> receiveConfig;

        public MassTransitInMemoryRelayModule(
            string queueName,
            Action<IReceiveEndpointConfigurator, IComponentContext> receiveConfig,
            TypesCatalog consumersAssemblies)
        : base(consumersAssemblies)
        {
            this.queueName = queueName;
            this.receiveConfig = receiveConfig;
        }

        protected override IBusControl CreateBus(IComponentContext context)
        {
            var bus = Bus.Factory.CreateUsingInMemory(cfg =>
            {
                cfg.UseSerilog();
                cfg.UseRetry(retryConfig => retryConfig.Incremental(5, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10)));

                cfg.ReceiveEndpoint(queueName, rcv => receiveConfig(rcv, context));
            });

            return bus;
        }

        public static void DefaultReceiveEndpointConfig(IReceiveEndpointConfigurator config, IComponentContext context)
        {
            config.UseInMemoryOutbox();
            config.ConfigureConsumers(context);
        }
    }
}
