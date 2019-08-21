using System;

namespace LeanCode.DomainModels.Model
{
    /// <summary> Represents a Domain Event </summary>
    public interface IDomainEvent
    {
        Guid Id { get; }
        DateTime DateOccurred { get; }
    }
}
