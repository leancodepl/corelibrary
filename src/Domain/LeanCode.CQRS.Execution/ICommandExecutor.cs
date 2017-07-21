using System.Security.Claims;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface ICommandExecutor
    {
        Task<CommandResult> RunAsync<TCommand>(
            ClaimsPrincipal user, TCommand command)
            where TCommand : ICommand;
    }
}
