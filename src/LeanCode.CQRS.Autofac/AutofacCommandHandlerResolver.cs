using System;
using Autofac;

namespace LeanCode.CQRS.Autofac
{
    public class AutofacCommandHandlerResolver : ICommandHandlerResolver
    {
        private readonly Func<IComponentContext> componentContext;

        public AutofacCommandHandlerResolver(Func<IComponentContext> componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICommandHandler<TCommand> FindCommandHandler<TCommand>() where TCommand : ICommand
        {
            ICommandHandler<TCommand> handler;
            componentContext().TryResolve<ICommandHandler<TCommand>>(out handler);
            return handler;
        }
    }
}
