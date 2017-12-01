using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface IQueryHandlerResolver<TAppContext>
    {
        IQueryHandlerWrapper FindQueryHandler(Type queryType);
    }

    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface IQueryHandlerWrapper
    {
        Task<object> ExecuteAsync(object context, IQuery query);
    }
}
