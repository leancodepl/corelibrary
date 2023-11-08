using LeanCode.CodeAnalysis.Tests.TestSamples;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CodeAnalysis.Tests.Data;

public class FirstQueryQH : IQueryHandler<FirstQuery, int>
{
    public Task<int> ExecuteAsync(HttpContext context, FirstQuery query) => throw new NotImplementedException();
}

public class MultipleQueriesQH : IQueryHandler<FirstQuery, int>, IQueryHandler<SecondQuery, int>
{
    public Task<int> ExecuteAsync(HttpContext context, FirstQuery query) => throw new NotImplementedException();

    public Task<int> ExecuteAsync(HttpContext context, SecondQuery query) => throw new NotImplementedException();
}
