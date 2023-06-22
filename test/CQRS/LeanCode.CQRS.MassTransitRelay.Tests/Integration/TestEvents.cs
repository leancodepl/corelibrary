using LeanCode.DomainModels.Model;
using TimeProvider = LeanCode.Time.TimeProvider;

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
        DateOccurred = TimeProvider.Now;
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
        DateOccurred = TimeProvider.Now;
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
        DateOccurred = TimeProvider.Now;
        CorrelationId = correlationId;
    }
}
