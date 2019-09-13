using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface ICommandExecutor<TAppContext>
    {
        Task<CommandResult> RunAsync(TAppContext appContext, ICommand command);
    }
}
