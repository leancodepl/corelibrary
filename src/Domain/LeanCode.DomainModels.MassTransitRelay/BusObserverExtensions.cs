using System.Collections.Generic;
using Autofac;
using MassTransit;
using MassTransit.BusConfigurators;
using MassTransit.Transports;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public static class BusObserverExtensions
    {
        public static void ConnectBusObservers(this IBusObserverConnector bus, IComponentContext context)
        {
            var observers = context.Resolve<IEnumerable<IBusObserver>>();
            foreach (var obs in observers)
            {
                bus.ConnectBusObserver(obs);
            }
        }

        public static void ConnectReceiveEndpointObservers(this IReceiveEndpointObserverConnector rcv, IComponentContext context)
        {
            var recvObservers = context.Resolve<IEnumerable<IReceiveEndpointObserver>>();
            foreach (var obs in recvObservers)
            {
                rcv.ConnectReceiveEndpointObserver(obs);
            }
        }
    }
}
