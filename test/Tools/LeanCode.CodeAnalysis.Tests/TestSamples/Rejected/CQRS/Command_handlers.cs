using LeanCode.CodeAnalysis.Tests.TestSamples.Accepted.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CodeAnalysis.Tests.TestSamples.Rejected.CQRS.WrongNamespace;

public class WrongCommandHandlerName : ICommandHandler<FirstCommand>
{
    public Task ExecuteAsync(HttpContext context, FirstCommand command) => throw new NotImplementedException();
}

public class WrongMultipleCommandHandlerName : ICommandHandler<FirstCommand>, ICommandHandler<SecondCommand>
{
    public Task ExecuteAsync(HttpContext context, FirstCommand command) => throw new NotImplementedException();

    public Task ExecuteAsync(HttpContext context, SecondCommand command) => throw new NotImplementedException();
}
