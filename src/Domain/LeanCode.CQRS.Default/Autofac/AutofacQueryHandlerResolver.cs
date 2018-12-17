using System;
using System.Linq;
using System.Reflection;
using Autofac;
using LeanCode.CQRS.Default.Wrappers;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.Default.Autofac
{
    class AutofacQueryHandlerResolver<TAppContext> : IQueryHandlerResolver<TAppContext>
    {
        private static readonly Type AppContextType = typeof(TAppContext);

        private static readonly Type HandlerBase = typeof(IQueryHandler<,,>);
        private static readonly Type HandlerWrapperBase = typeof(QueryHandlerWrapper<,,>);
        private static readonly TypesCache typesCache = new TypesCache(GetTypes, HandlerBase, HandlerWrapperBase);

        private readonly IComponentContext componentContext;

        public AutofacQueryHandlerResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public IQueryHandlerWrapper FindQueryHandler(Type queryType)
        {
            var cached = typesCache.Get(queryType);
            if (componentContext.TryResolve(cached.HandlerType, out var handler))
            {
                var wrapper = cached.Constructor.Invoke(new[] { handler });
                return (IQueryHandlerWrapper)wrapper;
            }
            else
            {
                return null;
            }
        }

        private static Type[] GetTypes(Type queryType)
        {
            var types = queryType
                .GetInterfaces()
                .Where(i =>
                    i.IsConstructedGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IQuery<>))
                .Single()
                .GenericTypeArguments;
            var resultType = types[0];
            return new[] { AppContextType, queryType, resultType };
        }
    }
}
