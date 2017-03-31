using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS
{
    public interface IQueryExecutor
    {
        Task<TResult> QueryAsync<TResult>(IQuery<TResult> query);
    }

    public class QueryHandlerNotFoundException : Exception
    {
        public Type QueryType { get; }

        public QueryHandlerNotFoundException(Type queryType)
            : base($"Cannot find handler for query {queryType.Name}.")
        {
            QueryType = queryType;
        }
    }
}
