using System.Threading.Tasks;
using LeanCode.Contracts;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Integration;

[AllowUnauthorized]
public class TestCommand : ICommand
{
    public Guid CorrelationId { get; set; }
}

public class TestCommandHandler : ICommandHandler<Context, TestCommand>
{
    private readonly TestDbContext dbContext;

    public TestCommandHandler(TestDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task ExecuteAsync(Context context, TestCommand command)
    {
        DomainEvents.Raise(new Event1(command.CorrelationId));
        HandledLog.Report(dbContext, command.CorrelationId, nameof(TestCommandHandler));
        return Task.CompletedTask;
    }
}
