using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface ICommandHandler<in TContext, in TCommand>
        where TCommand : ICommand<TContext>
    {
        Task ExecuteAsync(TContext context, TCommand command);
    }
}
