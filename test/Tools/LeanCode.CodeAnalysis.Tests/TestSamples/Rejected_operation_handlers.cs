using LeanCode.CodeAnalysis.Tests.TestSamples;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CodeAnalysis.Tests.Data;

public class WrongOperationHandlerName : IOperationHandler<FirstOperation, int>
{
    public Task<int> ExecuteAsync(HttpContext context, FirstOperation operation) => throw new NotImplementedException();
}

public class WrongMultipleOperationHandlerName : IOperationHandler<FirstOperation, int>, IOperationHandler<SecondOperation, int>
{
    public Task<int> ExecuteAsync(HttpContext context, FirstOperation operation) => throw new NotImplementedException();
    public Task<int> ExecuteAsync(HttpContext context, SecondOperation operation) => throw new NotImplementedException();
}
