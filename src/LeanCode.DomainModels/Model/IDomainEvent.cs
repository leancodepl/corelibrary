using System;

namespace LeanCode.DomainModels.Model
{
    public interface IDomainEvent
    {
        Guid Id { get; }
        DateTime DateOccurred { get; }
    }
}
