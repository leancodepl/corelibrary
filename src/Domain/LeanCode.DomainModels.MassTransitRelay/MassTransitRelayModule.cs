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
        private readonly string queueName;
        private readonly TypesCatalog consumers;

        protected abstract IBusControl CreateBus(Action<IBusFactoryConfigurator> configurator);

        public MassTransitRelayModuleBase(string queueName, TypesCatalog consumers)
        {
            this.queueName = queueName;
            this.consumers = consumers;
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
                cfg.AddConsumers(consumers.Assemblies);
                cfg.AddBus(CreateBus);
            });
        }

        private IBusControl CreateBus(IComponentContext context)
        {
            return CreateBus(cfg =>
            {
                var scopeFactory = context.Resolve<Func<ILifetimeScope>>();

                cfg.UseSerilog();
                cfg.UseRetry(retryConfig => retryConfig.Incremental(5, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10)));
                cfg.UseLifetimeScopeInjection(scopeFactory);
                cfg.UseDomainEventsPublishing();

                cfg.ReceiveEndpoint(queueName, rcv =>
                {
                    rcv.ConfigureConsumers(context);
                });
            });
        }
    }

    public class MassTransitInMemoryRelayModule : MassTransitRelayModuleBase
    {
        public MassTransitInMemoryRelayModule(string queueName, TypesCatalog consumersAssemblies)
            : base(queueName, consumersAssemblies)
        {
        }

        protected override IBusControl CreateBus(Action<IBusFactoryConfigurator> configurator)
        {
            return Bus.Factory.CreateUsingInMemory(configurator);
        }
    }
}
