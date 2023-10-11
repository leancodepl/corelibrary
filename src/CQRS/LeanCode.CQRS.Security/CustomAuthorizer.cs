using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Security;

public interface IHttpContextCustomAuthorizer
{
    Task<bool> CheckIfAuthorizedAsync(HttpContext context, object obj, object? customData);
}

public interface ICustomAuthorizer : IHttpContextCustomAuthorizer
{
    Task<bool> CheckIfAuthorizedAsync(
        ClaimsPrincipal user,
        object obj,
        object? customData,
        CancellationToken cancellationToken = default
    );

    Task<bool> IHttpContextCustomAuthorizer.CheckIfAuthorizedAsync(
        HttpContext context,
        object obj,
        object? customData
    ) => CheckIfAuthorizedAsync(context.User, obj, customData, context.RequestAborted);
}

public abstract class HttpContextCustomAuthorizer<TObject> : IHttpContextCustomAuthorizer
{
    public Task<bool> CheckIfAuthorizedAsync(HttpContext context, object obj, object? customData) =>
        CheckIfAuthorizedAsync(context, (TObject)obj);

    protected abstract Task<bool> CheckIfAuthorizedAsync(HttpContext context, TObject obj);
}

public abstract class CustomAuthorizer<TObject> : ICustomAuthorizer
{
    public Task<bool> CheckIfAuthorizedAsync(
        ClaimsPrincipal user,
        object obj,
        object? customData,
        CancellationToken cancellationToken = default
    ) => CheckIfAuthorizedAsync(user, (TObject)obj, cancellationToken);

    protected abstract Task<bool> CheckIfAuthorizedAsync(
        ClaimsPrincipal user,
        TObject obj,
        CancellationToken cancellationToken
    );
}

public abstract class HttpContextCustomAuthorizer<TObject, TCustomData> : IHttpContextCustomAuthorizer
{
    public Task<bool> CheckIfAuthorizedAsync(HttpContext context, object obj, object? customData) =>
        CheckIfAuthorizedInternalAsync(context, (TObject)obj, customData);

    protected abstract Task<bool> CheckIfAuthorizedAsync(HttpContext context, TObject obj, TCustomData? customData);

    private Task<bool> CheckIfAuthorizedInternalAsync(HttpContext context, TObject obj, object? customData)
    {
        if (!(customData is null || customData is TCustomData))
        {
            throw new ArgumentException(
                $"{GetType()} requires {typeof(TCustomData)} as custom data.",
                nameof(customData)
            );
        }

        return CheckIfAuthorizedAsync(context, obj, (TCustomData?)customData);
    }
}

public abstract class CustomAuthorizer<TObject, TCustomData> : ICustomAuthorizer
    where TCustomData : class
{
    public Task<bool> CheckIfAuthorizedAsync(
        ClaimsPrincipal user,
        object obj,
        object? customData,
        CancellationToken cancellationToken = default
    ) => CheckIfAuthorizedInternalAsync(user, (TObject)obj, customData, cancellationToken);

    protected abstract Task<bool> CheckIfAuthorizedAsync(
        ClaimsPrincipal user,
        TObject obj,
        TCustomData? customData,
        CancellationToken cancellationToken
    );

    private Task<bool> CheckIfAuthorizedInternalAsync(
        ClaimsPrincipal user,
        TObject obj,
        object? customData,
        CancellationToken cancellationToken
    )
    {
        if (!(customData is null || customData is TCustomData))
        {
            throw new ArgumentException(
                $"{GetType()} requires {typeof(TCustomData)} as custom data.",
                nameof(customData)
            );
        }

        return CheckIfAuthorizedAsync(user, obj, (TCustomData?)customData, cancellationToken);
    }
}
