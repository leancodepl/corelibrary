using System.Security.Claims;
using LeanCode.Contracts;
using LeanCode.CQRS.AspNetCore.Registration;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Local;

public class MiddlewareBasedLocalCommandExecutor : MiddlewareBasedLocalExecutor, ILocalCommandExecutor
{
    public MiddlewareBasedLocalCommandExecutor(
        IServiceProvider serviceProvider,
        ICQRSObjectSource objectSource,
        Action<ICQRSApplicationBuilder> configure
    )
        : base(serviceProvider, objectSource, configure) { }

    public Task<CommandResult> RunAsync<T>(
        T command,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default
    )
        where T : ICommand => RunInternalAsync<CommandResult>(command, user, null, cancellationToken);

    public Task<CommandResult> RunAsync<T>(
        T command,
        ClaimsPrincipal user,
        IHeaderDictionary headers,
        CancellationToken cancellationToken = default
    )
        where T : ICommand => RunInternalAsync<CommandResult>(command, user, headers, cancellationToken);
}
