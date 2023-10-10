using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration;

[HttpContextCustomAuthorizeWhen]
public class TestOperation : IOperation<TestOperationResult>, IHttpContextCustomAuthorizerParams
{
    public bool FailAuthorization { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
}

public class TestOperationResult
{
    public int Sum { get; set; }
}

public class TestOperationHandler : IOperationHandler<TestOperation, TestOperationResult>
{
    public Task<TestOperationResult> ExecuteAsync(HttpContext context, TestOperation operation)
    {
        return Task.FromResult(new TestOperationResult { Sum = operation.X + operation.Y });
    }
}
