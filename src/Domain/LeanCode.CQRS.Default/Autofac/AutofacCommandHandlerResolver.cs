using System;
using System.Linq;
using Autofac;
using LeanCode.CQRS.Default.Wrappers;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.Default.Autofac
{
    class AutofacCommandHandlerResolver<TAppContext> : ICommandHandlerResolver<TAppContext>
    {
        private static readonly Type AppContextType = typeof(TAppContext);

        private static readonly Type HandlerBase = typeof(ICommandHandler<,>);
        private static readonly Type HandlerWrapperBase = typeof(CommandHandlerWrapper<,>);
        private static readonly TypesCache typesCache = new TypesCache(GetTypes, HandlerBase, HandlerWrapperBase);

        private readonly IComponentContext componentContext;

        public AutofacCommandHandlerResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICommandHandlerWrapper FindCommandHandler(Type commandType)
        {
            var cached = typesCache.Get(commandType);
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

        private static Type[] GetTypes(Type commandType)
        {
            return new[] { AppContextType, commandType };
        }
    }
}
