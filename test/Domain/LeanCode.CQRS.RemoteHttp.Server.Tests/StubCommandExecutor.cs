using System.Security.Claims;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class StubCommandExecutor : ICommandExecutor<AppContext>
    {
        public static readonly ValidationError SampleError = new ValidationError("Prop", "999", 2);

        public AppContext LastContext { get; private set; }
        public ICommand LastCommand { get; private set; }

        public Task<CommandResult> RunAsync<TCommand>(
            AppContext context, TCommand command)
            where TCommand : ICommand
        {
            LastContext = context;
            LastCommand = command;
            if (LastCommand is SampleRemoteCommand cmd)
            {
                if (cmd.Prop == 999)
                {
                    return Task.FromResult(CommandResult.NotValid(
                        new ValidationResult(new[]
                        {
                            SampleError
                        })
                    ));
                }
            }
            return Task.FromResult(CommandResult.Success());
        }
    }
}
