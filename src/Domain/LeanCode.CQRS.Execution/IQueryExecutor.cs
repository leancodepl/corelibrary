using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface IQueryExecutor
    {
        Task<TResult> GetAsync<TContext, TResult>(TContext context, IQuery<TContext, TResult> query);
    }
}
