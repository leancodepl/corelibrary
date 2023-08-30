using MassTransit;

namespace Leancode.CQRS.MassTransitRelay;

public static class ConsumerConfigurationExtensions
{
    public const string ActorIdentifier = "ActorId";

    public static ISendPipeConfigurator ConfigureSendActorIdPropagation(ISendPipeConfigurator cfg)
    {
        cfg.UseExecute(ex =>
        {
            if (
                ex.TryGetPayload<ConsumeContext>(out var payload)
                && payload.TryGetHeader<string>(ActorIdentifier, out var actorId)
            )
            {
                ex.Headers.Set(ActorIdentifier, actorId);
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
                && payload.TryGetHeader<string>(ActorIdentifier, out var actorId)
            )
            {
                ex.Headers.Set(ActorIdentifier, actorId);
            }
        });
        return cfg;
    }
}
