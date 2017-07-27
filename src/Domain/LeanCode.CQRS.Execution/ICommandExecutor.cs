using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface ICommandExecutor<TContext>
    {
        Task<CommandResult> RunAsync<TCommand>(TContext context, TCommand command)
            where TCommand : ICommand;
    }
}
