using System.Threading.Tasks;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    class StubCommandExecutor : ICommandExecutor
    {
        public ICommand LastCommand { get; private set; }

        public Task<CommandResult> ExecuteAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            LastCommand = command;
            return Task.FromResult(CommandResult.Success());
        }
    }
}
