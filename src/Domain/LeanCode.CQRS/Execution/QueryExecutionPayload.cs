namespace LeanCode.CQRS.Execution
{
    public class QueryExecutionPayload
    {
        public object Context { get; }
        public IQuery Query { get; }

        public QueryExecutionPayload(object context, IQuery query)
        {
            Context = context;
            Query = query;
        }
    }
}
