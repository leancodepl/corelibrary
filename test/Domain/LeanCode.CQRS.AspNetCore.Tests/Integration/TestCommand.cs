using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration;

public class TestCommand : ICommand
{
    public string Param { get; set; }
}

public class TestCommandHandler : ICommandHandler<TestCommand>
{
    public Task ExecuteAsync(HttpContext context, TestCommand command)
    {
        return Task.CompletedTask;
    }
}
