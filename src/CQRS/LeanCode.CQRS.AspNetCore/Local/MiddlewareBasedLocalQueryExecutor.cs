using System.Security.Claims;
using LeanCode.Contracts;
using LeanCode.CQRS.AspNetCore.Registration;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Local;

public class MiddlewareBasedLocalQueryExecutor : MiddlewareBasedLocalExecutor, ILocalQueryExecutor
{
    public MiddlewareBasedLocalQueryExecutor(
        IServiceProvider serviceProvider,
        ICQRSObjectSource objectSource,
        Action<ICQRSApplicationBuilder> configure
    )
        : base(serviceProvider, objectSource, configure) { }

    public Task<TResult> GetAsync<TResult>(
        IQuery<TResult> query,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default
    ) => RunInternalAsync<TResult>(query, user, null, cancellationToken);

    public Task<TResult> GetAsync<TResult>(
        IQuery<TResult> query,
        ClaimsPrincipal user,
        IHeaderDictionary headers,
        CancellationToken cancellationToken = default
    ) => RunInternalAsync<TResult>(query, user, headers, cancellationToken);
}
