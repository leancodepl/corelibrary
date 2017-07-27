using System;

namespace LeanCode.CQRS.Execution
{
    public class CommandHandlerNotFoundException : Exception
    {
        public Type ContextType { get; }
        public Type CommandType { get; }

        public CommandHandlerNotFoundException(Type contextType, Type commandType)
            : base($"Cannot find handler for command {commandType.Name} executed with context {contextType.Name}.")
        {
            CommandType = commandType;
        }
    }
}
