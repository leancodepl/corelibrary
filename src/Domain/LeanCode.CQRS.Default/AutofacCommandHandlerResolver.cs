using System;
using System.Collections.Concurrent;
using System.Reflection;
using Autofac;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.Default
{
    class AutofacCommandHandlerResolver : ICommandHandlerResolver
    {
        private static readonly Type HandlerBase = typeof(ICommandHandler<>);
        private static readonly Type HandlerWrapperBase = typeof(CommandHandlerWrapper<>);

        private static readonly ConcurrentDictionary<Type, Tuple<Type, ConstructorInfo>> typesCache =
            new ConcurrentDictionary<Type, Tuple<Type, ConstructorInfo>>();

        private readonly IComponentContext componentContext;

        public AutofacCommandHandlerResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICommandHandlerWrapper FindCommandHandler(Type commandType)
        {
            var cached = typesCache.GetOrAdd(commandType, _ =>
            {
                var queryHandlerType = HandlerBase.MakeGenericType(commandType);
                var wrappedHandlerType = HandlerWrapperBase.MakeGenericType(commandType);
                var ctor = wrappedHandlerType.GetConstructors()[0];
                return Tuple.Create(queryHandlerType, ctor);
            });

            componentContext.TryResolve(cached.Item1, out var handler);

            if (handler == null)
            {
                return null;
            }
            return (ICommandHandlerWrapper)cached.Item2.Invoke(new[] { handler });
        }
    }
}
