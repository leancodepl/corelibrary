using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation;
using LeanCode.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Middleware;

[SuppressMessage(category: "?", "CA1034", Justification = "Nesting public types for better tests separation")]
public sealed class CQRSValidationMiddlewareTests : CQRSMiddlewareTestBase<CQRSValidationMiddleware>
{
    private readonly ICommandValidatorWrapper validator;
    private readonly ICommandValidatorResolver validatorResolver;

    public CQRSValidationMiddlewareTests()
    {
        validatorResolver = Substitute.For<ICommandValidatorResolver>();
        validator = Substitute.For<ICommandValidatorWrapper>();

        validatorResolver.FindCommandValidator(typeof(UnvalidatedCommand)).Returns(null as ICommandValidatorWrapper);
        validatorResolver.FindCommandValidator(typeof(ValidatedCommand)).Returns(validator);

        FinalPipeline = ctx =>
        {
            ctx.SetCQRSExecutionResult(ExecutionResult.WithPayload(CommandResult.Success));
            return Task.CompletedTask;
        };
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(_ => validatorResolver);
        services.AddLogging(logging => logging.AddNullLeanCodeLogger());
    }

    [Fact]
    public async Task Ignores_commands_without_validator()
    {
        var ctx = await SendAsync(new UnvalidatedCommand());

        var commandResult = ctx.ShouldHaveResponseStatusCode(StatusCodes.Status200OK)
            .ShouldHaveResponseContentType(Serializer.ContentType)
            .ShouldContainExecutionResult(StatusCodes.Status200OK)
            .ShouldContainCommandResult();
        commandResult.ShouldBeSuccessful();

        Serializer.ShouldHaveSerialized(commandResult);

        VerifyActivity("middleware - Validation");
    }

    [Fact]
    public async Task Continues_execution_if_validation_passed()
    {
        var cmd = new ValidatedCommand();
        SetValidationResult(cmd, new ValidationResult(null));

        var ctx = await SendAsync(cmd);
        AssertCommandResultSuccess(ctx);
    }

    [Fact]
    public async Task Stops_execution_if_validation_did_not_pass()
    {
        var error1 = new ValidationError("PropertyName1", "ErrorMessage1", 11);
        var error2 = new ValidationError("PropertyName2", "ErrorMessage2", 12);

        var cmd = new ValidatedCommand();
        SetValidationResult(cmd, new ValidationResult([error1, error2]));

        var ctx = await SendAsync(cmd);

        AssertCommandResultFailure(ctx, error1, error2);
    }

    private void SetValidationResult(ICommand command, ValidationResult result)
    {
        validator.ValidateAsync(Arg.Any<HttpContext>(), command).Returns(result);
    }

    private void AssertCommandResultSuccess(HttpContext httpContext)
    {
        var commandResult = httpContext
            .ShouldHaveResponseStatusCode(StatusCodes.Status200OK)
            .ShouldHaveResponseContentType(Serializer.ContentType)
            .ShouldContainExecutionResult(StatusCodes.Status200OK)
            .ShouldContainCommandResult();
        commandResult.ShouldBeSuccessful();

        Serializer.ShouldHaveSerialized(commandResult);

        VerifyActivity("middleware - Validation", activityStatusCode: ActivityStatusCode.Ok);
        VerifyNoCQRSSuccessMetrics();
        VerifyNoCQRSFailureMetrics();
    }

    private void AssertCommandResultFailure(HttpContext httpContext, params ValidationError[] errors)
    {
        var commandResult = httpContext
            .ShouldHaveResponseStatusCode(StatusCodes.Status422UnprocessableEntity)
            .ShouldHaveResponseContentType(Serializer.ContentType)
            .ShouldContainExecutionResult(StatusCodes.Status422UnprocessableEntity)
            .ShouldContainCommandResult();
        commandResult.ShouldFailWithValidationErrors(errors);

        Serializer.ShouldHaveSerialized(commandResult);

        VerifyActivity("middleware - Validation");
        VerifyCQRSFailureMetrics(CQRSMetrics.ValidationFailure, 1);
    }

    private Task<HttpContext> SendAsync(ICommand command)
    {
        return Server.SendAsync(ctx =>
        {
            var cqrsMetadata = new CQRSObjectMetadata(
                CQRSObjectKind.Command,
                command.GetType(),
                typeof(CommandResult),
                typeof(IgnoreType),
                (_, _) => Task.FromResult<object?>(null)
            );

            ctx.SetEndpoint(TestHelpers.MockCQRSEndpoint(cqrsMetadata));
            ctx.SetCQRSRequestPayload(command);
        });
    }

    private sealed class UnvalidatedCommand : ICommand;

    private sealed class ValidatedCommand : ICommand;

    private sealed class IgnoreType;
}
