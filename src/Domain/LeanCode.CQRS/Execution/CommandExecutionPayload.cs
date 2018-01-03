namespace LeanCode.CQRS.Execution
{
    public struct CommandExecutionPayload : IExecutionPayload<ICommand>
    {
        public object Context { get; }
        public ICommand Object { get; }
        object IExecutionPayload.Object => Object;

        public CommandExecutionPayload(object context, ICommand obj)
        {
            Context = context;
            Object = obj;
        }
    }
}
