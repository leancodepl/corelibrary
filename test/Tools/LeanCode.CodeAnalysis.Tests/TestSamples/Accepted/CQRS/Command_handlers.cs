using LeanCode.CodeAnalysis.Tests.TestSamples.Accepted.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CodeAnalysis.Tests.TestSamples.Accepted.CQRS;

public class FirstCommandCH : ICommandHandler<FirstCommand>
{
    public Task ExecuteAsync(HttpContext context, FirstCommand command) => throw new NotImplementedException();
}

public class MultipleCommandsCH : ICommandHandler<FirstCommand>, ICommandHandler<SecondCommand>
{
    public Task ExecuteAsync(HttpContext context, FirstCommand command) => throw new NotImplementedException();

    public Task ExecuteAsync(HttpContext context, SecondCommand command) => throw new NotImplementedException();
}
