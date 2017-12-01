using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface IQueryExecutor<TAppContext>
    {
        Task<TResult> GetAsync<TContext, TResult>(
            TAppContext appContext,
            TContext context,
            IQuery<TContext, TResult> query);
    }
}
