using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface ICommandExecutor<TAppContext>
    {
        Task<CommandResult> RunAsync<TContext, TCommand>(
            TAppContext appContext,
            TContext context,
            TCommand command)
            where TCommand : ICommand<TContext>;
    }
}
