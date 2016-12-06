using System.Threading.Tasks;

namespace LeanCode.CQRS
{
    public interface IRemoteCommandExecutor
    {
        Task<CommandResult> ExecuteCommand<TCommand>(TCommand command)
            where TCommand : IRemoteCommand;
    }
}
