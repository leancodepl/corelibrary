using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface ICommandHandler<in TContext, TCommand>
        where TCommand : ICommand
    {
        Task ExecuteAsync(TContext context, TCommand command);
    }

    public abstract class CommandHandler<TCommand>
        : ICommandHandler<object, TCommand>
        where TCommand : ICommand
    {
        Task ICommandHandler<object, TCommand>.ExecuteAsync(object context, TCommand command)
        {
            return ExecuteAsync(command);
        }

        protected abstract Task ExecuteAsync(TCommand command);
    }
}
