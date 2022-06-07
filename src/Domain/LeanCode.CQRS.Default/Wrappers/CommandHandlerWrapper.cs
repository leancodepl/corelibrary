using System.Threading.Tasks;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.Default.Wrappers
{
    internal class CommandHandlerWrapper<TAppContext, TCommand> : ICommandHandlerWrapper
        where TCommand : ICommand
    {
        private readonly ICommandHandler<TAppContext, TCommand> handler;

        public CommandHandlerWrapper(ICommandHandler<TAppContext, TCommand> handler)
        {
            this.handler = handler;
        }

        public Task ExecuteAsync(object context, ICommand command) =>
            handler.ExecuteAsync((TAppContext)context, (TCommand)command);
    }
}
