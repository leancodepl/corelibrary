using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface IQueryExecutor
    {
        Task<TResult> GetAsync<TResult>(IQuery<TResult> query);
    }
}
