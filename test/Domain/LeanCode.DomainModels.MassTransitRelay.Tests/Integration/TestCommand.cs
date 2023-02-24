using System.Threading.Tasks;
using LeanCode.Contracts;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Integration;

[AllowUnauthorized]
public class TestCommand : ICommand { }

public class TestCommandHandler : ICommandHandler<Context, TestCommand>
{
    public Task ExecuteAsync(Context context, TestCommand command)
    {
        DomainEvents.Raise(new Event1());
        return Task.CompletedTask;
    }
}
