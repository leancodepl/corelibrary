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

    public async Task<CommandResult> RunAsync<T>(
        T command,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default
    )
        where T : ICommand => (CommandResult)(await RunInternalAsync(command, user, null, cancellationToken))!;

    public async Task<CommandResult> RunAsync<T>(
        T command,
        ClaimsPrincipal user,
        IHeaderDictionary headers,
        CancellationToken cancellationToken = default
    )
        where T : ICommand => (CommandResult)(await RunInternalAsync(command, user, headers, cancellationToken))!;
}
