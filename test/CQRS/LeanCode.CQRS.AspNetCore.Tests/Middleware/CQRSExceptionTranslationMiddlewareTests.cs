using FluentAssertions;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.Execution;
using LeanCode.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Middleware;

public sealed class CQRSExceptionTranslationMiddlewareTests : CQRSMiddlewareTestBase<CQRSExceptionTranslationMiddleware>
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(logging => logging.AddNullLeanCodeLogger());
    }

    [Fact]
    public async Task If_CommandExecutionInvalidException_is_thrown_its_translated()
    {
        FinalPipeline = _ => throw new CommandExecutionInvalidException(23, "error message");

        var httpContext = await SendAsync();

        var commandResult = httpContext
            .ShouldHaveResponseStatusCode(StatusCodes.Status422UnprocessableEntity)
            .ShouldHaveResponseContentType(Serializer.ContentType)
            .ShouldContainExecutionResult(StatusCodes.Status422UnprocessableEntity)
            .ShouldContainCommandResult();
        commandResult.ShouldFailWithValidationErrors(new ValidationError("", "error message", 23));

        Serializer.ShouldHaveSerialized(commandResult);

        VerifyCQRSFailureMetrics(CQRSMetrics.ValidationFailure, 1);
    }

    [Fact]
    public async Task If_other_exception_is_thrown_its_propagated()
    {
        FinalPipeline = _ => throw new InvalidOperationException("error message");

        var errorRequest = SendAsync;
        await errorRequest.Should().ThrowAsync<InvalidOperationException>();

        VerifyCQRSFailureMetrics(CQRSMetrics.ValidationFailure, 0);
    }

    [Fact]
    public async Task Regular_flow_is_not_interrupted()
    {
        FinalPipeline = ctx =>
        {
            ctx.SetCQRSExecutionResult(ExecutionResult.WithPayload(CommandResult.Success));
            return Task.CompletedTask;
        };

        var httpContext = await SendAsync();

        var commandResult = httpContext
            .ShouldHaveResponseStatusCode(StatusCodes.Status200OK)
            .ShouldHaveResponseContentType(Serializer.ContentType)
            .ShouldContainExecutionResult(StatusCodes.Status200OK)
            .ShouldContainCommandResult();
        commandResult.ShouldBeSuccessful();

        Serializer.ShouldHaveSerialized(commandResult);

        VerifyCQRSFailureMetrics(CQRSMetrics.ValidationFailure, 0);
    }

    private Task<HttpContext> SendAsync()
    {
        return Server.SendAsync(ctx =>
        {
            var cqrsMetadata = new CQRSObjectMetadata(
                CQRSObjectKind.Command,
                typeof(Command),
                typeof(CommandResult),
                typeof(Ignore),
                (_, _) => Task.FromResult<object?>(null)
            );

            ctx.SetEndpoint(TestHelpers.MockCQRSEndpoint(cqrsMetadata));
            ctx.SetCQRSRequestPayload(new Command());
        });
    }

    private sealed class Command : ICommand;

    private sealed class Ignore;
}
