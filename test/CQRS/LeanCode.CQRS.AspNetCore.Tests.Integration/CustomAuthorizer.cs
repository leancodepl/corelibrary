using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration;

public interface ICustomAuthorizerParams
{
    public bool FailAuthorization { get; set; }
}

[SuppressMessage("?", "CA1040", Justification = "Marker interface")]
public interface ICustomAuthorizer { }

public class CustomAuthorizer : CustomAuthorizer<ICustomAuthorizerParams>, ICustomAuthorizer
{
    protected override Task<bool> CheckIfAuthorizedAsync(
        ClaimsPrincipal user,
        ICustomAuthorizerParams obj,
        CancellationToken ct
    )
    {
        return Task.FromResult(!obj.FailAuthorization);
    }
}

public sealed class CustomAuthorizeWhenAttribute : AuthorizeWhenAttribute
{
    public CustomAuthorizeWhenAttribute()
        : base(typeof(ICustomAuthorizer)) { }
}
