using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Autofac;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Default.Wrappers;

namespace LeanCode.CQRS.Default.Autofac
{
    class AutofacQueryHandlerResolver : IQueryHandlerResolver
    {
        private static readonly Type QueryHandlerBase = typeof(IQueryHandler<,>);
        private static readonly Type QueryHandlerWrapperBase = typeof(QueryHandlerWrapper<,>);

        private static readonly ConcurrentDictionary<Type, Tuple<Type, ConstructorInfo>> typesCache =
            new ConcurrentDictionary<Type, Tuple<Type, ConstructorInfo>>();

        private readonly IComponentContext componentContext;

        public AutofacQueryHandlerResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public IQueryHandlerWrapper FindQueryHandler(Type queryType)
        {
            var cached = typesCache.GetOrAdd(queryType, _ =>
            {
                var resultType = GetResultType(queryType);
                var queryHandlerType = QueryHandlerBase.MakeGenericType(queryType, resultType);
                var wrappedHandlerType = QueryHandlerWrapperBase.MakeGenericType(queryType, resultType);
                var ctor = wrappedHandlerType.GetConstructors()[0];
                return Tuple.Create(queryHandlerType, ctor);
            });

            componentContext.TryResolve(cached.Item1, out var handler);

            if (handler == null)
            {
                return null;
            }
            return (IQueryHandlerWrapper)cached.Item2.Invoke(new[] { handler });
        }

        private static Type GetResultType(Type queryType)
        {
            var q = queryType
                .GetInterfaces()
                .Where(i =>
                    i.IsConstructedGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IQuery<>))
                .Single();
            return q.GenericTypeArguments[0];
        }
    }
}
