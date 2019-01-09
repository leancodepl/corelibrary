using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface ICommandHandler<in TAppContext, in TCommand>
        where TCommand : ICommand
    {
        Task ExecuteAsync(TAppContext context, TCommand command);
    }
}
