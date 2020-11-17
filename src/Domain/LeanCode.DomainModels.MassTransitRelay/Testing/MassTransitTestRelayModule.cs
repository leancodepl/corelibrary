using System;
using Autofac;
using LeanCode.Components;
using MassTransit;
using MassTransit.Testing;

namespace LeanCode.DomainModels.MassTransitRelay.Testing
{
    public class MassTransitTestRelayModule : AppModule
    {
        private readonly TimeSpan inactivityTimeout;

        public MassTransitTestRelayModule()
        {
            this.inactivityTimeout = TimeSpan.FromSeconds(1);
        }

        public MassTransitTestRelayModule(TimeSpan inactivityTimeout)
        {
            this.inactivityTimeout = inactivityTimeout;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => c.Resolve<IBusControl>().CreateBusActivityMonitor(inactivityTimeout))
                .AutoActivate()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
