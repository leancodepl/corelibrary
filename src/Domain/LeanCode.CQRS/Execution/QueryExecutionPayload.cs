namespace LeanCode.CQRS.Execution
{
    public struct QueryExecutionPayload : IExecutionPayload<IQuery>
    {
        public object Context { get; }
        public IQuery Object { get; }
        object IExecutionPayload.Object => Object;

        public QueryExecutionPayload(object context, IQuery obj)
        {
            Context = context;
            Object = obj;
        }
    }
}
