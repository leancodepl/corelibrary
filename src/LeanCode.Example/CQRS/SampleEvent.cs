using System;
using LeanCode.DomainModels.Model;

namespace LeanCode.Example.CQRS
{
    public class SampleEvent : IDomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime DateOccurred { get; } = DateTime.UtcNow;
    }
}