using System;

namespace LeanCode.CQRS.Execution;

public class QueryHandlerNotFoundException : Exception
{
    public Type QueryType { get; }

    public QueryHandlerNotFoundException(Type queryType)
        : base($"Cannot find handler for query {queryType.Name}.")
    {
        QueryType = queryType;
    }
}
