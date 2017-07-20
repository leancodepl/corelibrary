using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        Task ExecuteAsync(TCommand command);
    }
}
