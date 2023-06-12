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
                "`HttpContext` is not available in validation context. Ensure you are calling the validator through FluentValidationCommandValidatorAdapter."
            );
        }
    }

    public static T GetService<T>(this IValidationContext ctx)
        where T : notnull
    {
        return ctx.HttpContext().RequestServices.GetRequiredService<T>();
    }
}
