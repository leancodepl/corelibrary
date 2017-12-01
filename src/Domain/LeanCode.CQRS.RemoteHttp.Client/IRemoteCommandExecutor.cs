using System.Threading.Tasks;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public interface IRemoteCommandExecutor
    {
        Task<CommandResult> RunAsync<TContext>(IRemoteCommand<TContext> command);
    }
}
