using LeanCode.Contracts;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration;

public class TestCommand : ICommand
{
    public string Param { get; set; }
}

public class TestCommandHandler : ICommandHandler<TestContext, TestCommand>
{
    public Task ExecuteAsync(TestContext context, TestCommand command)
    {
        return Task.CompletedTask;
    }
}
