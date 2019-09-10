using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface IQueryExecutor<TAppContext>
    {
        Task<TResult> GetAsync<TResult>(TAppContext appContext, IQuery<TResult> query);
    }
}
