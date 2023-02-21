using System.Threading.Tasks;
using LeanCode.Contracts;

namespace LeanCode.CQRS.Execution;

public interface ICommandExecutor<TAppContext>
{
    Task<CommandResult> RunAsync(TAppContext appContext, ICommand command);
}
