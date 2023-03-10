using System;
using System.Linq;
using Autofac;
using LeanCode.Contracts;
using LeanCode.CQRS.Default.Wrappers;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default.Autofac;

internal sealed class AutofacOperationHandlerResolver<TAppContext> : IOperationHandlerResolver<TAppContext>
    where TAppContext : notnull, IPipelineContext
{
    private static readonly Type AppContextType = typeof(TAppContext);

    private static readonly Type HandlerBase = typeof(IOperationHandler<,,>);
    private static readonly Type HandlerWrapperBase = typeof(OperationHandlerWrapper<,,>);
    private static readonly TypesCache TypesCache = new(GetTypes, HandlerBase, HandlerWrapperBase);

    private readonly IComponentContext componentContext;

    public AutofacOperationHandlerResolver(IComponentContext componentContext)
    {
        this.componentContext = componentContext;
    }

    public IOperationHandlerWrapper? FindOperationHandler(Type operationType)
    {
        var (handlerType, constructor) = TypesCache.Get(operationType);

        if (componentContext.TryResolve(handlerType, out var handler))
        {
            var wrapper = constructor.Invoke(new[] { handler });

            return (IOperationHandlerWrapper)wrapper;
        }
        else
        {
            return null;
        }
    }

    private static Type[] GetTypes(Type operationType)
    {
        var types = operationType
            .GetInterfaces()
            .Where(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IOperation<>))
            .Single()
            .GenericTypeArguments;

        var resultType = types[0];

        return new[] { AppContextType, operationType, resultType };
    }
}
