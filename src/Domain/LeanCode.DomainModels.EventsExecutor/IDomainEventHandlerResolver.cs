using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.EventsExecutor
{
    public interface IDomainEventHandlerResolver
    {
        IReadOnlyList<IDomainEventHandlerWrapper> FindEventHandlers(Type eventType);
    }

    public interface IDomainEventHandlerWrapper
    {
        Type UnderlyingHandler { get; }
        Task HandleAsync(IDomainEvent domainEvent);
    }
}
