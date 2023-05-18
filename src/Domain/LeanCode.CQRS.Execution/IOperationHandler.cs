using System.Threading.Tasks;
using LeanCode.Contracts;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Execution;

public interface IOperationHandler<in TOperation, TResult>
    where TOperation : IOperation<TResult>
{
    public Task<TResult> ExecuteAsync(HttpContext context, TOperation operation);
}
