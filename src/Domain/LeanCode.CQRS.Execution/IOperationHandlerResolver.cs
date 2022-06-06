using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Execution
{
    public interface IOperationHandlerResolver<TAppContext>
        where TAppContext : notnull, IPipelineContext
    {
        IOperationHandlerWrapper? FindOperationHandler(Type operationType);
    }

    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface IOperationHandlerWrapper
    {
        Task<object?> ExecuteAsync(object context, IOperation query);
    }
}
