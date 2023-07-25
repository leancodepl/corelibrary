using LeanCode.DomainModels.Model;
using LeanCode.TimeProvider;

namespace LeanCode.CQRS.MassTransitRelay.Tests.Integration;

public class Event1 : IDomainEvent
{
    public DateTime DateOccurred { get; }
    public Guid Id { get; }
    public Guid CorrelationId { get; }

    public Event1(Guid correlationId)
    {
        CorrelationId = correlationId;
        Id = Guid.NewGuid();
        DateOccurred = Time.UtcNow;
    }
}

public class Event2 : IDomainEvent
{
    public DateTime DateOccurred { get; }
    public Guid Id { get; }
    public Guid CorrelationId { get; }

    public Event2(Guid correlationId)
    {
        Id = Guid.NewGuid();
        DateOccurred = Time.UtcNow;
        CorrelationId = correlationId;
    }
}

public class Event3 : IDomainEvent
{
    public DateTime DateOccurred { get; }
    public Guid Id { get; }
    public Guid CorrelationId { get; }

    public Event3(Guid correlationId)
    {
        Id = Guid.NewGuid();
        DateOccurred = Time.UtcNow;
        CorrelationId = correlationId;
    }
}
