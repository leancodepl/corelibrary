using System;
using System.Reflection;
using Autofac;
using GreenPipes;
using LeanCode.Components;
using LeanCode.DomainModels.MassTransitRelay.Middleware;
using MassTransit;
using MassTransit.AutofacIntegration;

namespace LeanCode.DomainModels.MassTransitRelay.Testing
{
    public class MassTransitTestRelayModule : AppModule
    {
        private readonly TimeSpan inactivityWaitTime;

        public MassTransitTestRelayModule()
        {
            inactivityWaitTime = TimeSpan.FromSeconds(1);
        }

        public MassTransitTestRelayModule(TimeSpan inactivityWaitTime)
        {
            this.inactivityWaitTime = inactivityWaitTime;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => ResettableBusActivityMonitor.CreateFor(c.Resolve<IBusControl>(), inactivityWaitTime))
                .AutoActivate()
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        public static void TestBusConfigurator(IContainerBuilderBusConfigurator busCfg)
        {
            busCfg.UsingInMemory((context, config) =>
            {
                var queueName = Assembly.GetEntryAssembly()!.GetName().Name;

                config.ReceiveEndpoint(queueName, rcv =>
                {
                    rcv.UseLogsCorrelation();
                    rcv.UseRetry(retryConfig => retryConfig.Immediate(5));
                    rcv.UseConsumedMessagesFiltering(context);
                    rcv.StoreAndPublishDomainEvents(context);

                    rcv.ConfigureConsumers(context);
                    rcv.ConnectReceiveEndpointObservers(context);
                });

                config.ConnectBusObservers(context);
            });
        }
    }
}
