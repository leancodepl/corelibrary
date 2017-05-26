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
            componentContext.TryResolve<ICommandHandler<TCommand>>(out var handler);
            return handler;
        }
    }
}
