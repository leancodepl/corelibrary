using System.Threading.Tasks;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.Default.Wrappers
{
    class CommandHandlerWrapper<TContext, TCommand> : ICommandHandlerWrapper
        where TCommand : ICommand<TContext>
    {
        private readonly ICommandHandler<TContext, TCommand> handler;

        public CommandHandlerWrapper(ICommandHandler<TContext, TCommand> handler)
        {
            this.handler = handler;
        }

        public Task ExecuteAsync(object context, ICommand command)
        {
            return handler.ExecuteAsync((TContext)context, (TCommand)command);
        }
    }
}
