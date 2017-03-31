using System.Threading.Tasks;

namespace LeanCode.CQRS
{
    public interface IQueryHandlerResolver
    {
        IQueryHandlerWrapper<TResult> FindQueryHandler<TResult>(IQuery<TResult> query);
    }

    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface IQueryHandlerWrapper<TResult>
    {
        Task<TResult> ExecuteAsync(IQuery<TResult> query);
    }
}
