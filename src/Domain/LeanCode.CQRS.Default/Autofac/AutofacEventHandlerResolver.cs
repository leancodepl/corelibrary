using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using LeanCode.CQRS.Default.Wrappers;
using LeanCode.DomainModels.EventsExecution;

namespace LeanCode.CQRS.Default.Autofac
{
    public class AutofacEventHandlerResolver : IDomainEventHandlerResolver
    {
        private static readonly Type EnumerableType = typeof(IEnumerable<>);
        private static readonly Type EventHandlerBase = typeof(IDomainEventHandler<>);
        private static readonly Type EventHandlerWrapperBase = typeof(EventHandlerWrapper<>);
        private static readonly TypesCache typesCache = new TypesCache(
            a => EnumerableType.MakeGenericType(EventHandlerBase.MakeGenericType(a)),
            a => EventHandlerWrapperBase.MakeGenericType(a));

        private readonly IComponentContext componentContext;

        public AutofacEventHandlerResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public IReadOnlyList<IDomainEventHandlerWrapper> FindEventHandlers(Type eventType)
        {
            var cached = typesCache.Get(eventType);
            object handlers;
            componentContext.TryResolve(cached.Item1, out handlers);
            return ((IEnumerable<object>)handlers)
                .Select(h => (IDomainEventHandlerWrapper)cached.Item2.Invoke(new[] { h }))
                .ToList();
        }
    }
}
