using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface ICommandExecutor<TAppContext>
    {
        /// <summary>
        /// Executes handler for the command.
        /// </summary>
        Task<CommandResult> RunAsync<TContext>(
            TAppContext appContext,
            TContext context,
            ICommand<TContext> command);

        /// <summary>
        /// Executes handler for the command, creating context using <see cref="IObjectContextFromAppContextFactory{TAppContext, TContext}" />.
        /// </summary>
        Task<CommandResult> RunAsync<TContext>(
            TAppContext appContext,
            ICommand<TContext> command);
    }
}
