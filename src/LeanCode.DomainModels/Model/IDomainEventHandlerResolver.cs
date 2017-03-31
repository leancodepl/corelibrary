using System;
using System.Collections.Generic;

namespace LeanCode.DomainModels.Model
{
    public interface IDomainEventHandlerResolver
    {
        IReadOnlyList<IDomainEventHandlerWrapper> FindEventHandlers(Type eventType);
    }

    public interface IDomainEventHandlerWrapper
    {
        void Handle(IDomainEvent @event);
    }
}
