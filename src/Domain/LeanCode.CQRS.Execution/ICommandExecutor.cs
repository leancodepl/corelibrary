using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface ICommandExecutor
    {
        Task<CommandResult> RunAsync<TContext, TCommand>(TContext context, TCommand command)
            where TCommand : ICommand<TContext>;
    }
}
