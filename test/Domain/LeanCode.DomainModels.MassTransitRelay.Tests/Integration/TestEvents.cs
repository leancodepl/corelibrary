using System;
using LeanCode.DomainModels.Model;
using LeanCode.Time;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Integration
{
    public class Event1 : IDomainEvent
    {
        public DateTime DateOccurred { get; }
        public Guid Id { get; }

        public Event1()
        {
            Id = Guid.NewGuid();
            DateOccurred = TimeProvider.Now;
        }
    }

    public class Event2 : IDomainEvent
    {
        public DateTime DateOccurred { get; }
        public Guid Id { get; }

        public Event2()
        {
            Id = Guid.NewGuid();
            DateOccurred = TimeProvider.Now;
        }
    }

    public class Event3 : IDomainEvent
    {
        public DateTime DateOccurred { get; }
        public Guid Id { get; }

        public Event3()
        {
            Id = Guid.NewGuid();
            DateOccurred = TimeProvider.Now;
        }
    }
}
