using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration;

[CustomAuthorizeWhen]
public class TestQuery : IQuery<TestQueryResult>, ICustomAuthorizerParams
{
    public bool FailAuthorization { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
}

public class TestQueryResult
{
    public int Sum { get; set; }
}

public class TestQueryHandler : IQueryHandler<TestQuery, TestQueryResult>
{
    public Task<TestQueryResult> ExecuteAsync(HttpContext context, TestQuery query)
    {
        return Task.FromResult(new TestQueryResult { Sum = query.X + query.Y });
    }
}
