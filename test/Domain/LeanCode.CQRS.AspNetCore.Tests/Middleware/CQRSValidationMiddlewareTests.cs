using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Middleware;

public class CQRSValidationMiddlewareTests : IDisposable, IAsyncLifetime
{
    private readonly IHost host;
    private readonly TestServer server;
    private readonly ICommandValidatorResolver validatorResolver;
    private readonly ICommandValidatorWrapper validator;

    public CQRSValidationMiddlewareTests()
    {
        validatorResolver = Substitute.For<ICommandValidatorResolver>();
        validator = Substitute.For<ICommandValidatorWrapper>();

        validatorResolver.FindCommandValidator(typeof(UnvalidatedCommand)).Returns(null as ICommandValidatorWrapper);
        validatorResolver.FindCommandValidator(typeof(ValidatedCommand)).Returns(validator);

        host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(cfg =>
                    {
                        cfg.AddSingleton(validatorResolver);
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<CQRSValidationMiddleware>();
                        app.Run(ctx =>
                        {
                            var payload = ctx.GetCQRSRequestPayload();
                            payload.SetResult(ExecutionResult.Success(CommandResult.Success));
                            return Task.CompletedTask;
                        });
                    });
            })
            .Build();

        server = host.GetTestServer();
    }

    [Fact]
    public async Task Ignores_commands_without_validator()
    {
        var ctx = await SendAsync(new UnvalidatedCommand());
        AssertCommandResultSuccess(ctx);
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
        SetValidationResult(cmd, new ValidationResult(new[] { error1, error2 }));

        var ctx = await SendAsync(cmd);

        AssertCommandResultFailure(ctx, error1, error2);
    }

    private void SetValidationResult(ICommand command, ValidationResult result)
    {
        validator.ValidateAsync(Arg.Any<HttpContext>(), command).Returns(result);
    }

    private void AssertCommandResultSuccess(HttpContext httpContext)
    {
        var rawResult = httpContext.GetCQRSRequestPayload().Result?.Payload;
        var commandResult = Assert.IsType<CommandResult>(rawResult);

        Assert.True(commandResult.WasSuccessful);
        Assert.Empty(commandResult.ValidationErrors);
    }

    private void AssertCommandResultFailure(HttpContext httpContext, params ValidationError[] errors)
    {
        var rawResult = httpContext.GetCQRSRequestPayload().Result?.Payload;
        var commandResult = Assert.IsType<CommandResult>(rawResult);

        Assert.False(commandResult.WasSuccessful);
    }

    private Task<HttpContext> SendAsync(ICommand command)
    {
        return server.SendAsync(ctx =>
        {
            var cqrsMetadata = new CQRSObjectMetadata(
                CQRSObjectKind.Command,
                command.GetType(),
                typeof(CommandResult),
                typeof(IgnoreType)
            );

            var endpointMetadata = new CQRSEndpointMetadata(
                cqrsMetadata,
                (_, __) => Task.FromResult<object?>(CommandResult.Success)
            );
            var endpoint = new Endpoint(null, new EndpointMetadataCollection(endpointMetadata), null);

            ctx.SetEndpoint(endpoint);
            ctx.SetCQRSRequestPayload(command);
        });
    }

    private class UnvalidatedCommand : ICommand { }

    private class ValidatedCommand : ICommand { }

    private class IgnoreType { }

    public void Dispose()
    {
        server.Dispose();
        host.Dispose();
    }

    public Task InitializeAsync() => host.StartAsync();

    public Task DisposeAsync() => host.StopAsync();
}
