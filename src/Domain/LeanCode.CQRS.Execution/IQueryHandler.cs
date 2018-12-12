using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface IQueryHandler<in TContext, in TQuery, TResult>
        where TQuery : IQuery<TContext, TResult>
    {
        Task<TResult> ExecuteAsync(TContext context, TQuery query);
    }
}
