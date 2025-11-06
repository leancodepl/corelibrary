using System.Collections.Immutable;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using LeanCode.CQRS.Execution;
using LeanCode.Logging;
using LeanCode.OpenTelemetry;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class CQRSExceptionTranslationMiddleware
{
    private readonly CQRSMetrics metrics;
    private readonly RequestDelegate next;
    private readonly ILogger<CQRSExceptionTranslationMiddleware> logger;

    public CQRSExceptionTranslationMiddleware(
        CQRSMetrics metrics,
        RequestDelegate next,
        ILogger<CQRSExceptionTranslationMiddleware> logger
    )
    {
        this.metrics = metrics;
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var cqrsMetadata = httpContext.GetCQRSObjectMetadata();
        var cqrsPayload = httpContext.GetCQRSRequestPayload();

        if (cqrsMetadata.ObjectKind != CQRSObjectKind.Command)
        {
            throw new InvalidOperationException("CQRSExceptionTranslationMiddleware may be used only for commands.");
        }

        try
        {
            await next(httpContext);
        }
        catch (CommandExecutionInvalidException ex)
        {
            using var activity = LeanCodeActivitySource.StartMiddleware("ExceptionTranslation");
            activity?.SetTag("error.code", ex.ErrorCode);

            var result = WrapInCommandResult(ex);
            metrics.CQRSFailure(CQRSMetrics.ValidationFailure);
            logger.Warning("Command {@Command} is not valid with result {@Result}", cqrsPayload.Payload, result);

            var executionResult = ExecutionResult.WithPayload(result, StatusCodes.Status422UnprocessableEntity);
            await httpContext.CompleteCQRSExecutionResult(executionResult);
        }
    }

    private static CommandResult WrapInCommandResult(CommandExecutionInvalidException ex)
    {
        var error = new ValidationError(propertyName: "", ex.Message, ex.ErrorCode);
        return new CommandResult(ImmutableList.Create(error));
    }
}
