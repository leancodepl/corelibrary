namespace LeanCode.CQRS.MassTransitRelay.Middleware;

public record EventsPublisherOptions(string NameClaimType)
{
    public static readonly EventsPublisherOptions Default = new("sub");
};
