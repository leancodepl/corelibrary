using System.Diagnostics;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation;
using LeanCode.Logging;
using LeanCode.OpenTelemetry;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class CQRSValidationMiddleware
{
    private readonly CQRSMetrics metrics;
    private readonly RequestDelegate next;
    private readonly ILogger<CQRSValidationMiddleware> logger;

    public CQRSValidationMiddleware(CQRSMetrics metrics, RequestDelegate next, ILogger<CQRSValidationMiddleware> logger)
    {
        this.metrics = metrics;
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext, ICommandValidatorResolver resolver)
    {
        using var activity = LeanCodeActivitySource.StartMiddleware("Validation");
        var cqrsMetadata = httpContext.GetCQRSObjectMetadata();
        var payload = httpContext.GetCQRSRequestPayload();

        if (cqrsMetadata.ObjectKind != CQRSObjectKind.Command)
        {
            throw new InvalidOperationException("CQRSValidationMiddleware may be used only for commands");
        }

        var validator = resolver.FindCommandValidator(cqrsMetadata.ObjectType);
        if (validator is null)
        {
            activity?.SetTag("validation.validator_present", false);
            await next(httpContext);
            return;
        }

        activity?.SetTag("validation.validator_present", true);
        activity?.SetTag("validation.validator", validator.GetType().FullName);

        var result = await validator.ValidateAsync(httpContext, (ICommand)payload.Payload);
        activity?.SetTag("validation.valid", result.IsValid);

        if (!result.IsValid)
        {
            metrics.CQRSFailure(CQRSMetrics.ValidationFailure);
            logger.Warning("Command {@Command} is not valid with result {@Result}", payload.Payload, result);

            var commandResult = CommandResult.NotValid(result);
            httpContext.SetCQRSExecutionResult(
                ExecutionResult.WithPayload(commandResult, StatusCodes.Status422UnprocessableEntity)
            );
            return;
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
        await next(httpContext);
    }
}
