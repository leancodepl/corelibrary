using System.Security.Claims;
using LeanCode.Contracts;
using LeanCode.CQRS.AspNetCore.Registration;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Local;

public class MiddlewareBasedLocalOperationExecutor : MiddlewareBasedLocalExecutor, ILocalOperationExecutor
{
    public MiddlewareBasedLocalOperationExecutor(
        IServiceProvider serviceProvider,
        ICQRSObjectSource objectSource,
        Action<ICQRSApplicationBuilder> configure
    )
        : base(serviceProvider, objectSource, configure) { }

    public async Task<TResult> ExecuteAsync<TResult>(
        IOperation<TResult> query,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default
    ) => (TResult)(await RunInternalAsync(query, user, null, cancellationToken))!;

    public async Task<TResult> ExecuteAsync<TResult>(
        IOperation<TResult> query,
        ClaimsPrincipal user,
        IHeaderDictionary headers,
        CancellationToken cancellationToken = default
    ) => (TResult)(await RunInternalAsync(query, user, null, cancellationToken))!;
}
