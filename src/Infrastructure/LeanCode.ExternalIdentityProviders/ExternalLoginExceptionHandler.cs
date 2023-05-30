using System.Collections.Immutable;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace LeanCode.ExternalIdentityProviders;

public class ExternalLoginExceptionHandler
{
    private readonly RequestDelegate next;
    public const int ErrorCodeInvalidToken = 10000;
    public const int ErrorCodeOtherConnected = 10001;
    public const int ErrorCodeOther = 10002;

    public ExternalLoginExceptionHandler(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var cqrsMetadata = httpContext.GetCQRSEndpoint().ObjectMetadata;

        if (cqrsMetadata.ObjectKind != CQRSObjectKind.Command)
        {
            throw new InvalidOperationException("ExternalLoginExceptionHandler may be only used for commands.");
        }

        try
        {
            await next(httpContext);
        }
        catch (ExternalLoginException ex)
        {
            ValidationError err = ex.TokenValidation switch
            {
                TokenValidationError.Invalid => new("", "The token is invalid.", ErrorCodeInvalidToken),

                TokenValidationError.OtherConnected
                    => new("", "Other account is already connected with this token.", ErrorCodeOtherConnected),

                _ => new("", "Cannot perform external login.", ErrorCodeOther),
            };

            var result = new CommandResult(ImmutableList.Create(err));
            httpContext
                .GetCQRSRequestPayload()
                .SetResult(ExecutionResult.WithPayload(result, StatusCodes.Status422UnprocessableEntity));
        }
    }
}

public static class IApplicationBuilderExtensions
{
    public static IApplicationBuilder HandleExternalLoginExceptions(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExternalLoginExceptionHandler>();
    }
}
