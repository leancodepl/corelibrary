using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using LeanCode.DomainModels.Model;

namespace LeanCode.ComainModels.Autofac
{
    public class AutofacEventHandlerResolver : IDomainEventHandlerResolver
    {
        private static readonly Type EnumerableType = typeof(IEnumerable<>);
        private static readonly Type EventHandlerBase = typeof(IDomainEventHandler<>);
        private static readonly Type EventHandlerWrapperBase = typeof(EventHandlerWrapper<>);

        private readonly ConcurrentDictionary<Type, Tuple<Type, ConstructorInfo>> typesCache =
            new ConcurrentDictionary<Type, Tuple<Type, ConstructorInfo>>();

        private readonly Func<IComponentContext> componentContext;

        public AutofacEventHandlerResolver(Func<IComponentContext> componentContext)
        {
            this.componentContext = componentContext;
        }

        public IReadOnlyList<IDomainEventHandlerWrapper> FindEventHandlers(Type eventType)
        {
            var cached = typesCache.GetOrAdd(eventType, type =>
            {
                var queryHandlerType = EnumerableType.MakeGenericType(EventHandlerBase.MakeGenericType(type));
                var wrappedHandlerType = EventHandlerWrapperBase.MakeGenericType(type);
                var ctor = wrappedHandlerType.GetConstructors()[0];
                return Tuple.Create(queryHandlerType, ctor);
            });

            object handlers;
            componentContext().TryResolve(cached.Item1, out handlers);
            return ((IEnumerable<object>)handlers)
                .Select(h => (IDomainEventHandlerWrapper)cached.Item2.Invoke(new[] { h }))
                .ToList();
        }
    }
}
