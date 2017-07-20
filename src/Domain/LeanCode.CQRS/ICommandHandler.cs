using System.Threading.Tasks;

namespace LeanCode.CQRS
{
    public interface ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        Task ExecuteAsync(TCommand command);
    }

    public abstract class SyncCommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        Task ICommandHandler<TCommand>.ExecuteAsync(TCommand command)
        {
            Execute(command);
            return Task.FromResult<object>(null);
        }

        public abstract void Execute(TCommand command);
    }
}
