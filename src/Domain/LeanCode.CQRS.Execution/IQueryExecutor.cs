using System.Threading.Tasks;
using LeanCode.Contracts;

namespace LeanCode.CQRS.Execution
{
    public interface IQueryExecutor<TAppContext>
    {
        Task<TResult> GetAsync<TResult>(TAppContext appContext, IQuery<TResult> query);
    }
}
