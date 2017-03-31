using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeanCode.DomainModels.Model
{
    public interface IDomainEventHandlerResolver
    {
        IReadOnlyList<IDomainEventHandlerWrapper> FindEventHandlers(Type eventType);
    }

    public interface IDomainEventHandlerWrapper
    {
        Task HandleAsync(IDomainEvent domainEvent);
    }
}
