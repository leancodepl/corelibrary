using System;
using System.Collections.Concurrent;
using System.Reflection;
using Autofac;

namespace LeanCode.CQRS.Autofac
{
    class AutofacQueryHandlerResolver : IQueryHandlerResolver
    {
        private static readonly Type QueryHandlerBase = typeof(IQueryHandler<,>);
        private static readonly Type QueryHandlerWrapperBase = typeof(QueryHandlerWrapper<,>);

        private readonly ConcurrentDictionary<Type, Tuple<Type, ConstructorInfo>> typesCache =
            new ConcurrentDictionary<Type, Tuple<Type, ConstructorInfo>>();

        private readonly Func<IComponentContext> componentContext;

        public AutofacQueryHandlerResolver(Func<IComponentContext> componentContext)
        {
            this.componentContext = componentContext;
        }

        public IQueryHandlerWrapper<TResult> FindQueryHandler<TResult>(IQuery<TResult> query)
        {
            var queryType = query.GetType();
            var resultType = typeof(TResult);

            var cached = typesCache.GetOrAdd(queryType, _ =>
            {
                var queryHandlerType = QueryHandlerBase.MakeGenericType(queryType, resultType);
                var wrappedHandlerType = QueryHandlerWrapperBase.MakeGenericType(queryType, resultType);
                var ctor = wrappedHandlerType.GetConstructors()[0];
                return Tuple.Create(queryHandlerType, ctor);
            });

            object handler;
            componentContext().TryResolve(cached.Item1, out handler);

            if (handler == null)
            {
                return null;
            }
            return (IQueryHandlerWrapper<TResult>)cached.Item2.Invoke(new[] { handler });
        }
    }
}
