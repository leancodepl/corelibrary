using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Security;

public interface ICustomAuthorizer
{
    Task<bool> CheckIfAuthorizedAsync(HttpContext httpContext, object obj, object? customData);
}

public abstract class CustomAuthorizer<TObject> : ICustomAuthorizer
{
    public Task<bool> CheckIfAuthorizedAsync(HttpContext httpContext, object obj, object? customData) =>
        CheckIfAuthorizedAsync(httpContext, (TObject)obj);

    protected abstract Task<bool> CheckIfAuthorizedAsync(HttpContext httpContext, TObject obj);
}

public abstract class CustomAuthorizer<TObject, TCustomData> : ICustomAuthorizer
    where TCustomData : class
{
    public Task<bool> CheckIfAuthorizedAsync(HttpContext httpContext, object obj, object? customData) =>
        CheckIfAuthorizedInternalAsync(httpContext, (TObject)obj, customData);

    protected abstract Task<bool> CheckIfAuthorizedAsync(HttpContext httpContext, TObject obj, TCustomData? customData);

    private Task<bool> CheckIfAuthorizedInternalAsync(HttpContext httpContext, TObject obj, object? customData)
    {
        if (!(customData is null || customData is TCustomData))
        {
            throw new ArgumentException(
                $"{GetType()} requires {typeof(TCustomData)} as custom data.",
                nameof(customData)
            );
        }

        return CheckIfAuthorizedAsync(httpContext, obj, (TCustomData?)customData);
    }
}
