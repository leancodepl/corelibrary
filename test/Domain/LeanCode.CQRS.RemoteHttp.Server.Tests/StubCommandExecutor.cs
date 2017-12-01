using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class StubCommandExecutor : ICommandExecutor<AppContext>
    {
        public static readonly ValidationError SampleError = new ValidationError("Prop", "999", 2);

        public AppContext LastAppContext { get; private set; }
        public object LastContext { get; private set; }
        public ICommand LastCommand { get; private set; }

        public Task<CommandResult> RunAsync<TContext, TCommand>(
            AppContext appContext,
            TContext context,
            TCommand command)
            where TCommand : ICommand<TContext>
        {
            LastAppContext = appContext;
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
