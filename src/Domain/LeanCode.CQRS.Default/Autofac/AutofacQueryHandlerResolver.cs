using System;
using System.Linq;
using Autofac;
using LeanCode.Contracts;
using LeanCode.CQRS.Default.Wrappers;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.Default.Autofac;

internal class AutofacQueryHandlerResolver<TAppContext> : IQueryHandlerResolver<TAppContext>
{
    private static readonly Type AppContextType = typeof(TAppContext);

    private static readonly Type HandlerBase = typeof(IQueryHandler<,,>);
    private static readonly Type HandlerWrapperBase = typeof(QueryHandlerWrapper<,,>);
    private static readonly TypesCache TypesCache = new TypesCache(GetTypes, HandlerBase, HandlerWrapperBase);

    private readonly IComponentContext componentContext;

    public AutofacQueryHandlerResolver(IComponentContext componentContext)
    {
        this.componentContext = componentContext;
    }

    public IQueryHandlerWrapper? FindQueryHandler(Type queryType)
    {
        var (handlerType, constructor) = TypesCache.Get(queryType);

        if (componentContext.TryResolve(handlerType, out var handler))
        {
            var wrapper = constructor.Invoke(new[] { handler });

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
            .Where(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>))
            .Single()
            .GenericTypeArguments;

        var resultType = types[0];

        return new[] { AppContextType, queryType, resultType };
    }
}
