using LeanCode.CodeAnalysis.Tests.TestSamples;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CodeAnalysis.Tests.Data;

public class FirstCommandCH : ICommandHandler<FirstCommand>
{
    public Task ExecuteAsync(HttpContext context, FirstCommand command) => throw new NotImplementedException();
}

public class MultipleCommandsCH : ICommandHandler<FirstCommand>, ICommandHandler<SecondCommand>
{
    public Task ExecuteAsync(HttpContext context, FirstCommand command) => throw new NotImplementedException();
    public Task ExecuteAsync(HttpContext context, SecondCommand command) => throw new NotImplementedException();
}
