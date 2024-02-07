using System.Security.Claims;
using LeanCode.Contracts;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Local;

public interface ILocalOperationExecutor
{
    Task<TResult> ExecuteAsync<TResult>(
        IOperation<TResult> query,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default
    );

    Task<TResult> ExecuteAsync<TResult>(
        IOperation<TResult> query,
        ClaimsPrincipal user,
        IHeaderDictionary headers,
        CancellationToken cancellationToken = default
    );
}
