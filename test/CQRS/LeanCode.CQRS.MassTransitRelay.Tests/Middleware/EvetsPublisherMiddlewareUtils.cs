using LeanCode.Contracts;
using LeanCode.DomainModels.Model;

namespace LeanCode.CQRS.MassTransitRelay.Tests.Middleware;

public class TestCommand : ICommand { }

public class IgnoreType { }

public class TestEvent : IDomainEvent
{
    public Guid Id { get; private set; }
    public DateTime DateOccurred { get; private set; }

    public TestEvent()
    {
        Id = Guid.NewGuid();
        DateOccurred = DateTime.Now;
    }
}
