using System.Security.Claims;
using LeanCode.Contracts;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Local;

public interface ILocalQueryExecutor
{
    Task<TResult> GetAsync<TResult>(
        IQuery<TResult> query,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default
    );

    Task<TResult> GetAsync<TResult>(
        IQuery<TResult> query,
        ClaimsPrincipal user,
        IHeaderDictionary headers,
        CancellationToken cancellationToken = default
    );
}
