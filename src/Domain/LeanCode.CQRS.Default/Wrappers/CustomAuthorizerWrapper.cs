using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Wrappers;

internal sealed class CustomAuthorizerWrapper<TAppContext> : ICustomAuthorizerWrapper
    where TAppContext : notnull
{
    private readonly ICustomAuthorizer<TAppContext> authorizer;

    public Type UnderlyingAuthorizer { get; }

    public CustomAuthorizerWrapper(ICustomAuthorizer<TAppContext> authorizer)
    {
        this.authorizer = authorizer;

        UnderlyingAuthorizer = authorizer.GetType();
    }

    public Task<bool> CheckIfAuthorizedAsync(object appContext, object obj, object? customData) =>
        authorizer.CheckIfAuthorizedAsync((TAppContext)appContext, obj, customData);
}
