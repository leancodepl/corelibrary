using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS
{
    public interface ICommandExecutor
    {
        Task<CommandResult> RunAsync<TCommand>(TCommand command)
            where TCommand : ICommand;
    }

    public class CommandHandlerNotFoundException : Exception
    {
        public Type CommandType { get; }

        public CommandHandlerNotFoundException(Type commandType)
            : base($"Cannot find handler for command {commandType.Name}.")
        {
            CommandType = commandType;
        }
    }
}
