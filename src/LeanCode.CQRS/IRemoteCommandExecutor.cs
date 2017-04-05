using System.Threading.Tasks;

namespace LeanCode.CQRS
{
    public interface IRemoteCommandExecutor
    {
        Task<CommandResult> ExecuteAsync(IRemoteCommand command);
    }
}
