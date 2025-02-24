using System.Collections.Immutable;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using LeanCode.CQRS.Execution;
using LeanCode.OpenTelemetry;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class CQRSExceptionTranslationMiddleware
{
    private readonly Serilog.ILogger logger;
    private readonly CQRSMetrics metrics;
    private readonly RequestDelegate next;

    public CQRSExceptionTranslationMiddleware(Serilog.ILogger logger, CQRSMetrics metrics, RequestDelegate next)
    {
        this.logger = logger.ForContext<CQRSExceptionTranslationMiddleware>();
        this.metrics = metrics;
        this.next = next;
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
            logger.Warning("Command {@Command} is not valid with result {@Result}", cqrsPayload.Payload, result);
            var executionResult = ExecutionResult.WithPayload(result, StatusCodes.Status422UnprocessableEntity);
            cqrsPayload.SetResult(executionResult);
            metrics.CQRSFailure(CQRSMetrics.ValidationFailure);
        }
    }

    private static CommandResult WrapInCommandResult(CommandExecutionInvalidException ex)
    {
        var error = new ValidationError(propertyName: "", ex.Message, ex.ErrorCode);
        return new CommandResult(ImmutableList.Create(error));
    }
}
