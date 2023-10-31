using System.Collections.Immutable;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class CQRSExceptionTranslationMiddleware
{
    private readonly ILogger logger = Log.ForContext<CQRSExceptionTranslationMiddleware>();
    private readonly CQRSMetrics metrics;
    private readonly RequestDelegate next;

    public CQRSExceptionTranslationMiddleware(CQRSMetrics metrics, RequestDelegate next)
    {
        this.metrics = metrics;
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var cqrsMetadata = httpContext.GetCQRSEndpoint().ObjectMetadata;
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
