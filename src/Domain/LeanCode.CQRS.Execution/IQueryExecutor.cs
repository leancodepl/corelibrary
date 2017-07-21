using System.Security.Claims;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface IQueryExecutor
    {
        Task<TResult> GetAsync<TResult>(
            ClaimsPrincipal user, IQuery<TResult> query);
    }
}
