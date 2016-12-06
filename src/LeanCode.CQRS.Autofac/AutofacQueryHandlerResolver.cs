using System;
using Autofac;

namespace LeanCode.CQRS.Autofac
{
    public class AutofacQueryHandlerResolver : IQueryHandlerResolver
    {
        private readonly Func<IComponentContext> componentContext;

        public AutofacQueryHandlerResolver(Func<IComponentContext> componentContext)
        {
            this.componentContext = componentContext;
        }

        public IQueryHandlerWrapper<TResult> FindQueryHandler<TResult>(IQuery<TResult> query)
        {
            var queryType = query.GetType();

            var handlerType = typeof(IQueryHandler<,>)
                .MakeGenericType(queryType, typeof(TResult));

            object handler;
            componentContext().TryResolve(handlerType, out handler);
            return GetWrapper<TResult>(queryType, handler);
        }

        private IQueryHandlerWrapper<TResult> GetWrapper<TResult>(Type query, object handler)
        {
            // PERF: cache the type for the query
            var wrapperType = typeof(QueryHandlerWrapper<,>)
                .MakeGenericType(query, typeof(TResult));
            return (IQueryHandlerWrapper<TResult>)Activator.CreateInstance(wrapperType, handler);
        }
    }
}
