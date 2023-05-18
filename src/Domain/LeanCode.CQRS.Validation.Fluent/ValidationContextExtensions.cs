using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.Validation.Fluent;

public static class ValidationContextExtensions
{
    public const string HttpContextKey = "HttpContext";

    public static HttpContext HttpContext(this IValidationContext ctx)
    {
        if (ctx.RootContextData.TryGetValue(HttpContextKey, out var httpContext))
        {
            return (HttpContext)httpContext;
        }
        else
        {
            throw new InvalidOperationException(
                "Cannot use `HttpContext` extension method outside the `ContextualValidator` class."
            );
        }
    }

    public static T GetService<T>(this IValidationContext ctx)
        where T : notnull
    {
        if (ctx.RootContextData.TryGetValue(HttpContextKey, out var httpContext))
        {
            return ((HttpContext)httpContext).RequestServices.GetRequiredService<T>();
        }
        else
        {
            throw new InvalidOperationException(
                "Cannot use `HttpContext` extension method outside the `ContextualValidator` class."
            );
        }
    }
}
