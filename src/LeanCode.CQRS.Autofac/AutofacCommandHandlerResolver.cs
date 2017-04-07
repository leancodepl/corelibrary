using Autofac;

namespace LeanCode.CQRS.Autofac
{
    class AutofacCommandHandlerResolver : ICommandHandlerResolver
    {
        private readonly IComponentContext componentContext;

        public AutofacCommandHandlerResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICommandHandler<TCommand> FindCommandHandler<TCommand>() where TCommand : ICommand
        {
            ICommandHandler<TCommand> handler;
            componentContext.TryResolve<ICommandHandler<TCommand>>(out handler);
            return handler;
        }
    }
}
