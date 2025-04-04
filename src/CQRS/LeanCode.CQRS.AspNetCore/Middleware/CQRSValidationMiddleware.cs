using System.Diagnostics;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation;
using LeanCode.OpenTelemetry;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class CQRSValidationMiddleware
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<CQRSValidationMiddleware>();

    private readonly CQRSMetrics metrics;
    private readonly RequestDelegate next;

    public CQRSValidationMiddleware(CQRSMetrics metrics, RequestDelegate next)
    {
        this.metrics = metrics;
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, ICommandValidatorResolver resolver)
    {
        var cqrsMetadata = httpContext.GetCQRSObjectMetadata();
        var payload = httpContext.GetCQRSRequestPayload();

        if (cqrsMetadata.ObjectKind != CQRSObjectKind.Command)
        {
            throw new InvalidOperationException("CQRSValidationMiddleware may be used only for commands");
        }

        var validator = resolver.FindCommandValidator(cqrsMetadata.ObjectType);

        if (validator is not null)
        {
            using var activity = LeanCodeActivitySource.StartMiddleware("Validation");
            activity?.SetTag("validation.validator", validator.GetType().FullName);

            var result = await validator.ValidateAsync(httpContext, (ICommand)payload.Payload);
            activity?.SetTag("validation.valid", result.IsValid);

            if (!result.IsValid)
            {
                logger.Warning("Command {@Command} is not valid with result {@Result}", payload.Payload, result);
                var commandResult = CommandResult.NotValid(result);
                payload.SetResult(ExecutionResult.WithPayload(commandResult, StatusCodes.Status422UnprocessableEntity));
                metrics.CQRSFailure(CQRSMetrics.ValidationFailure);
                return;
            }
            else
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
        }

        await next(httpContext);
    }
}
