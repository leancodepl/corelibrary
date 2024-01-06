using LeanCode.Contracts;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Local;

public interface ILocalCommandExecutor
{
    Task<CommandResult> RunAsync<T>(HttpContext context, T command, CancellationToken cancellationToken = default)
        where T : ICommand;
}
