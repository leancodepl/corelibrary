using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface ICommandExecutor<TAppContext>
    {
        /// <summary>
        /// Executes handler for the command.
        /// </summary>
        Task<CommandResult> RunAsync<TContext, TCommand>(
            TAppContext appContext,
            TContext context,
            TCommand command)
            where TCommand : ICommand<TContext>;

        /// <summary>
        /// Executes handler for the command, creating context using <see cref="IObjectContextFromAppContextFactory{TAppContext, TContext}" />.
        /// </summary>
        Task<CommandResult> RunAsync<TContext, TCommand>(
            TAppContext appContext,
            TCommand command)
            where TCommand : ICommand<TContext>;
    }
}
