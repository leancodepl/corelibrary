namespace LeanCode.CQRS.Execution
{
    public interface IExecutionPayload
    {
        object Context { get; }
        object Object { get; }
    }

    public interface IExecutionPayload<TObj> : IExecutionPayload
    {
        new TObj Object { get; }
    }
}
