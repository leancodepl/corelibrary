using System.Threading.Tasks;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public interface IRemoteCommandExecutor
    {
        Task<CommandResult> RunAsync(IRemoteCommand command);
    }
}
