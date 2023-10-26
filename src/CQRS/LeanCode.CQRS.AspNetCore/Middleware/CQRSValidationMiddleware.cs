using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class CQRSValidationMiddleware
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<CQRSValidationMiddleware>();

    private readonly RequestDelegate next;

    public CQRSValidationMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, ICommandValidatorResolver resolver)
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
                var commandResult = CommandResult.NotValid(result);
                payload.SetResult(ExecutionResult.WithPayload(commandResult, StatusCodes.Status422UnprocessableEntity));
                CQRSMetrics.CQRSFailure(CQRSMetrics.ValidationFailure);
                return;
            }
        }

        await next(httpContext);
    }
}
