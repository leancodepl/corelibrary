using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.MassTransitRelay;

public static class BusObserverExtensions
{
    public static void ConnectBusObservers(this IBusObserverConnector bus, IBusRegistrationContext context)
    {
        var observers = context.GetServices<IBusObserver>();
        foreach (var obs in observers)
        {
            bus.ConnectBusObserver(obs);
        }
    }

    public static void ConnectReceiveEndpointObservers(this IReceiveEndpointObserverConnector rcv, IBusRegistrationContext context)
    {
        var recvObservers = context.GetServices<IReceiveEndpointObserver>();
        foreach (var obs in recvObservers)
        {
            rcv.ConnectReceiveEndpointObserver(obs);
        }
    }
}
