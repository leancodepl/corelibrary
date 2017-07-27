using System;

namespace LeanCode.CQRS.Execution
{
    public class QueryHandlerNotFoundException : Exception
    {
        public Type ContextType { get; }
        public Type QueryType { get; }

        public QueryHandlerNotFoundException(Type contextType, Type queryType)
            : base($"Cannot find handler for query {queryType.Name} executed with context {contextType.Name}.")
        {
            ContextType = contextType;
            QueryType = queryType;
        }
    }
}
