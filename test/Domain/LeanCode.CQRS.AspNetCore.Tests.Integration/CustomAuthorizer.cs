using LeanCode.Contracts.Security;
using LeanCode.CQRS.Security;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration;


public interface ICustomAuthorizer { }

public class CustomAuthorizer : CustomAuthorizer<TestCommand>, ICustomAuthorizer
{
    protected override Task<bool> CheckIfAuthorizedAsync(HttpContext httpContext, TestCommand obj)
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
