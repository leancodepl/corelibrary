using System;
using System.Threading.Tasks;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests;

public class StubCommandExecutor : ICommandExecutor<AppContext>
{
    public static readonly ValidationError SampleError = new ValidationError("Prop", "999", 2);

    public AppContext? LastAppContext { get; private set; }
    public ICommand? LastCommand { get; private set; }

    public Task<CommandResult> RunAsync(AppContext appContext, ICommand command)
    {
        LastAppContext = appContext;
        LastCommand = command;
        if (LastCommand is SampleRemoteCommand cmd && cmd.Prop == 999)
        {
            return Task.FromResult(CommandResult.NotValid(new ValidationResult(new[] { SampleError })));
        }

        return Task.FromResult(CommandResult.Success);
    }
}
