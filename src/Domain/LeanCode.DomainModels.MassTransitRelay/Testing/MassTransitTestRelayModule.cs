using Autofac;
using LeanCode.Components;
using MassTransit;
using MassTransit.Testing.Implementations;

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
                .As<IBusActivityMonitor>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
