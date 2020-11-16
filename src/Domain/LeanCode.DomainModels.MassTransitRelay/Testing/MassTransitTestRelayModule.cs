using System;
using Autofac;
using LeanCode.Components;
using MassTransit;
using MassTransit.Testing;

namespace LeanCode.DomainModels.MassTransitRelay.Testing
{
    public class MassTransitTestRelayModule : AppModule
    {
        public static readonly TimeSpan NoActivityGranule = TimeSpan.FromSeconds(1);

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => c.Resolve<IBusControl>().CreateBusActivityMonitor(NoActivityGranule))
                .AutoActivate()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
