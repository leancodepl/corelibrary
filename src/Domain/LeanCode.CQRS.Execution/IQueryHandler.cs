using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface IQueryHandler<in TContext, TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        Task<TResult> ExecuteAsync(TContext context, TQuery query);
    }

    public abstract class BaseQueryHandler<TQuery, TResult>
        : IQueryHandler<object, TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        Task<TResult> IQueryHandler<object, TQuery, TResult>.ExecuteAsync(object context, TQuery query)
        {
            return ExecuteAsync(query);
        }

        protected abstract Task<TResult> ExecuteAsync(TQuery query);
    }
}
