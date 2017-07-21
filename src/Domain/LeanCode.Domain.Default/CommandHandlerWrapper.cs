using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;

namespace LeanCode.Domain.Default
{
    class CommandHandlerWrapper<TCommand> : ICommandHandlerWrapper
        where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand> handler;

        public CommandHandlerWrapper(ICommandHandler<TCommand> handler)
        {
            this.handler = handler;
        }

        public Task ExecuteAsync(ICommand command)
        {
            return handler.ExecuteAsync((TCommand)command);
        }
    }
}
