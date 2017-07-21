using System;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.EventsExecution.Tests
{
    public class Event1 : IDomainEvent
    {
        public Guid Id { get; }

        public DateTime DateOccurred { get; }

        public Event1()
        {
            Id = Guid.NewGuid();
            DateOccurred = DateTime.UtcNow;
        }
    }

    public class Event2 : IDomainEvent
    {
        public Guid Id { get; }

        public DateTime DateOccurred { get; }

        public Event2()
        {
            Id = Guid.NewGuid();
            DateOccurred = DateTime.UtcNow;
        }
    }
}
