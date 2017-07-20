using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface ICommandExecutor
    {
        Task<CommandResult> RunAsync<TCommand>(TCommand command)
            where TCommand : ICommand;
    }
}
