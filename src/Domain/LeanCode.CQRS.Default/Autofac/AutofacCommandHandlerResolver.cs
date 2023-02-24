using System;
using Autofac;
using LeanCode.CQRS.Default.Wrappers;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.Default.Autofac;

internal class AutofacCommandHandlerResolver<TAppContext> : ICommandHandlerResolver<TAppContext>
{
    private static readonly Type AppContextType = typeof(TAppContext);

    private static readonly Type HandlerBase = typeof(ICommandHandler<,>);
    private static readonly Type HandlerWrapperBase = typeof(CommandHandlerWrapper<,>);
    private static readonly TypesCache TypesCache = new TypesCache(GetTypes, HandlerBase, HandlerWrapperBase);

    private readonly IComponentContext componentContext;

    public AutofacCommandHandlerResolver(IComponentContext componentContext)
    {
        this.componentContext = componentContext;
    }

    public ICommandHandlerWrapper? FindCommandHandler(Type commandType)
    {
        var (handlerType, constructor) = TypesCache.Get(commandType);

        if (componentContext.TryResolve(handlerType, out var handler))
        {
            var wrapper = constructor.Invoke(new[] { handler });

            return (ICommandHandlerWrapper)wrapper;
        }
        else
        {
            return null;
        }
    }

    private static Type[] GetTypes(Type commandType) => new[] { AppContextType, commandType };
}
