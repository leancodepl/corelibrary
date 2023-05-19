using LeanCode.Contracts;
using LeanCode.CQRS.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class CQRSValidationMiddleware
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<CQRSValidationMiddleware>();

    private readonly ICommandValidatorResolver resolver;
    private readonly RequestDelegate next;

    public CQRSValidationMiddleware(ICommandValidatorResolver resolver, RequestDelegate next)
    {
        this.resolver = resolver;
        this.next = next;
    }

    public async Task InvokeAsync(
        HttpContext httpContext
    )
    {
        var cqrsMetadata = httpContext.GetCQRSEndpoint().ObjectMetadata;
        var payload = httpContext.GetCQRSRequestPayload();

        if (cqrsMetadata.ObjectKind != CQRSObjectKind.Command)
        {
            throw new InvalidOperationException("CQRSValidationMiddleware may be used only for commands");
        }

        var validator = resolver.FindCommandValidator(cqrsMetadata.ObjectType);

        if (validator is not null)
        {
            var result = await validator.ValidateAsync(httpContext, (ICommand)payload.Payload);

            if (!result.IsValid)
            {
                logger.Warning("Command {@Command} is not valid with result {@Result}", payload.Payload, result);
                payload.SetResult(CommandResult.NotValid(result));
                return;
            }
        }

        await next(httpContext);
    }
}
