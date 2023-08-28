using LeanCode.CQRS.MassTransitRelay.Middleware;
using MassTransit;

namespace Leancode.CQRS.MassTransitRelay;

public static class ConsumerConfigurationExtensions
{
    public static ISendPipeConfigurator ConfigureSendActorIdPropagation(ISendPipeConfigurator cfg)
    {
        cfg.UseExecute(ex =>
        {
            if (
                ex.TryGetPayload<ConsumeContext>(out var payload)
                && payload.TryGetHeader<string>(EventsPublisherMiddleware.EventActorIdentifier, out var actorId)
            )
            {
                ex.Headers.Set(EventsPublisherMiddleware.EventActorIdentifier, actorId);
            }
        });
        return cfg;
    }

    public static IPublishPipeConfigurator ConfigurePublishActorIdPropagation(IPublishPipeConfigurator cfg)
    {
        cfg.UseExecute(ex =>
        {
            if (
                ex.TryGetPayload<ConsumeContext>(out var payload)
                && payload.TryGetHeader<string>(EventsPublisherMiddleware.EventActorIdentifier, out var actorId)
            )
            {
                ex.Headers.Set(EventsPublisherMiddleware.EventActorIdentifier, actorId);
            }
        });
        return cfg;
    }
}
