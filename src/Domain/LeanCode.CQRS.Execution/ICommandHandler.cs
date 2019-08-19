using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    /// <summary>Handler for <typeparamref name="TCommand"/></summary>
    /// <typeparam name="TCommand">Command to be executed</typeparam>
    /// <typeparam name="TContext">Object context</typeparam>
    public interface ICommandHandler<in TContext, in TCommand>
        where TCommand : ICommand<TContext>
    {
        Task ExecuteAsync(TContext context, TCommand command);
    }

    public abstract class NoContextCommandHandler<TCommand>
        : ICommandHandler<VoidContext, TCommand>
        where TCommand : ICommand<VoidContext>
    {
        public Task ExecuteAsync(VoidContext context, TCommand command)
        {
            return ExecuteAsync(command);
        }

        protected abstract Task ExecuteAsync(TCommand command);
    }
}
