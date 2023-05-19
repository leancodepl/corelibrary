using LeanCode.Contracts.Security;
using LeanCode.CQRS.Security;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration;

public interface ICustomAuthorizerParams
{
    public bool FailAuthorization { get; set; }
}

public interface ICustomAuthorizer { }

public class CustomAuthorizer : CustomAuthorizer<ICustomAuthorizerParams>, ICustomAuthorizer
{
    protected override Task<bool> CheckIfAuthorizedAsync(HttpContext httpContext, ICustomAuthorizerParams obj)
    {
        return Task.FromResult(!obj.FailAuthorization);
    }
}

public class CustomAuthorizeWhenAttribute : AuthorizeWhenAttribute
{
    public CustomAuthorizeWhenAttribute()
        : base(typeof(ICustomAuthorizer))
    { }
}
