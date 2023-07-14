using System.Diagnostics.CodeAnalysis;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.IntegrationTestHelpers.Tests.App;

public class AuthQuery : IQuery<AuthResult> { }

public class AuthResult
{
    public bool IsAuthenticated { get; set; }

    [SuppressMessage("?", "CA2227", Justification = "Public modifiable list as part of the contract")]
    public List<KeyValuePair<string, string>> Claims { get; set; } = default!;
}

public class AuthQueryHandler : IQueryHandler<AuthQuery, AuthResult>
{
    public Task<AuthResult> ExecuteAsync(HttpContext context, AuthQuery query)
    {
        var principal = context.User;

        var result = new AuthResult
        {
            IsAuthenticated = principal.Identity?.IsAuthenticated ?? false,
            Claims = principal.Claims.Select(c => KeyValuePair.Create(c.Type, c.Value)).ToList(),
        };

        return Task.FromResult(result);
    }
}
