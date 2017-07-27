using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface IQueryExecutor<TContext>
    {
        Task<TResult> GetAsync<TResult>(TContext context, IQuery<TResult> query);
    }
}
