using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface IQueryHandler<in TAppContext, in TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        Task<TResult> ExecuteAsync(TAppContext context, TQuery query);
    }
}
