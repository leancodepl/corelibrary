using LeanCode.CodeAnalysis.Tests.TestSamples.Accepted.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CodeAnalysis.Tests.TestSamples.Rejected.CQRS.WrongNamespace;

public class WrongQueryHandlerName : IQueryHandler<FirstQuery, bool>
{
    public Task<bool> ExecuteAsync(HttpContext context, FirstQuery query) => throw new NotImplementedException();
}

public class WrongMultipleQueryHandlerName : IQueryHandler<FirstQuery, bool>, IQueryHandler<SecondQuery, bool>
{
    public Task<bool> ExecuteAsync(HttpContext context, FirstQuery query) => throw new NotImplementedException();

    public Task<bool> ExecuteAsync(HttpContext context, SecondQuery query) => throw new NotImplementedException();
}
