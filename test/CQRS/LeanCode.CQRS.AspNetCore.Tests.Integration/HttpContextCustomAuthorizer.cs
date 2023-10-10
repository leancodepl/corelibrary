using System.Diagnostics.CodeAnalysis;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.Security;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration;

public interface IHttpContextCustomAuthorizerParams
{
    public bool FailAuthorization { get; set; }
}

[SuppressMessage("?", "CA1040", Justification = "Marker interface")]
public interface IHttpContextCustomAuthorizer { }

public class HttpContextCustomAuthorizer
    : HttpContextCustomAuthorizer<IHttpContextCustomAuthorizerParams>,
        IHttpContextCustomAuthorizer
{
    protected override Task<bool> CheckIfAuthorizedAsync(HttpContext context, IHttpContextCustomAuthorizerParams obj)
    {
        return Task.FromResult(!obj.FailAuthorization);
    }
}

public sealed class HttpContextCustomAuthorizeWhenAttribute : AuthorizeWhenAttribute
{
    public HttpContextCustomAuthorizeWhenAttribute()
        : base(typeof(IHttpContextCustomAuthorizer)) { }
}
