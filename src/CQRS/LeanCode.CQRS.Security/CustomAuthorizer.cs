using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Security;

public interface ICustomAuthorizer
{
    Task<bool> CheckIfAuthorizedAsync(ClaimsPrincipal user, object obj, object? customData);
}

public abstract class CustomAuthorizer<TObject> : ICustomAuthorizer
{
    public Task<bool> CheckIfAuthorizedAsync(ClaimsPrincipal user, object obj, object? customData) =>
        CheckIfAuthorizedAsync(user, (TObject)obj);

    protected abstract Task<bool> CheckIfAuthorizedAsync(ClaimsPrincipal user, TObject obj);
}

public abstract class CustomAuthorizer<TObject, TCustomData> : ICustomAuthorizer
    where TCustomData : class
{
    public Task<bool> CheckIfAuthorizedAsync(ClaimsPrincipal user, object obj, object? customData) =>
        CheckIfAuthorizedInternalAsync(user, (TObject)obj, customData);

    protected abstract Task<bool> CheckIfAuthorizedAsync(ClaimsPrincipal user, TObject obj, TCustomData? customData);

    private Task<bool> CheckIfAuthorizedInternalAsync(ClaimsPrincipal user, TObject obj, object? customData)
    {
        if (!(customData is null || customData is TCustomData))
        {
            throw new ArgumentException(
                $"{GetType()} requires {typeof(TCustomData)} as custom data.",
                nameof(customData)
            );
        }

        return CheckIfAuthorizedAsync(user, obj, (TCustomData?)customData);
    }
}
