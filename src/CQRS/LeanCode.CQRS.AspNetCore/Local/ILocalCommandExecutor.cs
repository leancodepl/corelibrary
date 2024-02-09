using System.Security.Claims;
using LeanCode.Contracts;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Local;

public interface ILocalCommandExecutor
{
    Task<CommandResult> RunAsync<T>(T command, ClaimsPrincipal user, CancellationToken cancellationToken = default)
        where T : ICommand;

    Task<CommandResult> RunAsync<T>(
        T command,
        ClaimsPrincipal user,
        IHeaderDictionary headers,
        CancellationToken cancellationToken = default
    )
        where T : ICommand;
}
