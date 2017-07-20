using System.Threading.Tasks;

namespace LeanCode.CQRS
{
    public interface IQueryHandler<in TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        Task<TResult> ExecuteAsync(TQuery query);
    }

    public abstract class SyncQueryHandler<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        Task<TResult> IQueryHandler<TQuery, TResult>.ExecuteAsync(TQuery query)
        {
            return Task.FromResult(Execute(query));
        }

        public abstract TResult Execute(TQuery query);
    }
}
