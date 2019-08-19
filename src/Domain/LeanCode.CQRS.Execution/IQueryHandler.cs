using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    /// <summary>Handler for <typeparamref name="TQuery"/></summary>
    /// <typeparam name="TCommand">Query to be executed</typeparam>
    /// <typeparam name="TContext">Object context</typeparam>
    public interface IQueryHandler<in TContext, in TQuery, TResult>
        where TQuery : IQuery<TContext, TResult>
    {
        Task<TResult> ExecuteAsync(TContext context, TQuery query);
    }

    public abstract class NoContextQueryHandler<TQuery, TResult>
        : IQueryHandler<VoidContext, TQuery, TResult>
        where TQuery : IQuery<VoidContext, TResult>
    {
        public Task<TResult> ExecuteAsync(VoidContext context, TQuery query)
        {
            return ExecuteAsync(query);
        }

        protected abstract Task<TResult> ExecuteAsync(TQuery query);
    }
}
