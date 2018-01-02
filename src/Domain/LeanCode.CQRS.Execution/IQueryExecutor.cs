using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface IQueryExecutor<TAppContext>
    {
        /// <summary>
        /// Executes handler for the query.
        /// </summary>
        Task<TResult> GetAsync<TContext, TResult>(
            TAppContext appContext,
            TContext context,
            IQuery<TContext, TResult> query);

        /// <summary>
        /// Executes handler for the query, creating context using <see cref="IObjectContextFromAppContextFactory{TAppContext, TContext}" />.
        /// </summary>
        Task<TResult> GetAsync<TContext, TResult>(
            TAppContext appContext,
            IQuery<TContext, TResult> query);
    }
}
