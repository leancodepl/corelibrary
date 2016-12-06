using System;
using System.Collections.Generic;

namespace LeanCode.DomainModels.Model
{
    public interface IDomainEventHandlerResolver
    {
        IReadOnlyList<IDomainEventHandler> FindEventHandlers(Type eventType);
    }
}
