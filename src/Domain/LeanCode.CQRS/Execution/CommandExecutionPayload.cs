namespace LeanCode.CQRS.Execution
{
    public class CommandExecutionPayload
    {
        public object Context { get; }
        public ICommand Command { get; }

        public CommandExecutionPayload(object context, ICommand command)
        {
            Context = context;
            Command = command;
        }
    }
}
