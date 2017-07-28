using System;
using Autofac;
using LeanCode.CQRS.Default.Wrappers;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.Default.Autofac
{
    class AutofacCommandHandlerResolver : ICommandHandlerResolver
    {
        private static readonly Type HandlerBase = typeof(ICommandHandler<,>);
        private static readonly Type HandlerWrapperBase = typeof(CommandHandlerWrapper<,>);
        private static readonly TypesCache typesCache = new TypesCache(HandlerBase, HandlerWrapperBase);

        private readonly IComponentContext componentContext;

        public AutofacCommandHandlerResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICommandHandlerWrapper FindCommandHandler(Type contextType, Type commandType)
        {
            var cached = typesCache.Get(contextType, commandType);
            if (componentContext.TryResolve(cached.HandlerType, out var handler))
            {
                var wrapper = cached.Constructor.Invoke(new[] { handler });
                return (ICommandHandlerWrapper)wrapper;
            }
            else
            {
                return null;
            }
        }
    }
}
