using LeanCode.Contracts;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.Execution;
using LeanCode.DomainModels.Model;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.MassTransitRelay.Tests.Integration;

[AllowUnauthorized]
public class TestCommand : ICommand
{
    public Guid CorrelationId { get; set; }
}

public class TestCommandHandler : ICommandHandler<TestCommand>
{
    private readonly TestDbContext dbContext;

    public TestCommandHandler(TestDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task ExecuteAsync(HttpContext context, TestCommand command)
    {
        DomainEvents.Raise(new Event1(command.CorrelationId));
        HandledLog.Report(dbContext, command.CorrelationId, nameof(TestCommandHandler));
        return Task.CompletedTask;
    }
}
