using FluentAssertions;
using LeanCode.Contracts;
using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Middleware;

public sealed class CQRSExceptionTranslationMiddlewareTests : IDisposable, IAsyncLifetime
{
    private readonly IHost host;
    private readonly TestServer server;

    private RequestDelegate finalPipeline;

    public CQRSExceptionTranslationMiddlewareTests()
    {
        finalPipeline = _ => Task.CompletedTask;
        host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseMiddleware<CQRSExceptionTranslationMiddleware>();
                        app.Run(ctx => finalPipeline(ctx));
                    });
            })
            .Build();
        server = host.GetTestServer();
    }

    [Fact]
    public async Task If_CommandExecutionInvalidException_is_thrown_its_translated()
    {
        finalPipeline = _ => throw new CommandExecutionInvalidException(23, "error message");

        var httpContext = await SendAsync();

        var executionResult = ExtractExecutionResult(httpContext);
        executionResult.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        var commandResult = ExtractCommandResult(executionResult);
        var error = commandResult.ValidationErrors.Should().ContainSingle().Which;
        error.ErrorCode.Should().Be(23);
        error.ErrorMessage.Should().Be("error message");
    }

    [Fact]
    public async Task If_other_exception_is_thrown_its_propagated()
    {
        finalPipeline = _ => throw new InvalidOperationException("error message");

        var errorRequest = SendAsync;
        await errorRequest.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Regular_flow_is_not_interrupted()
    {
        finalPipeline = ctx =>
        {
            ctx.GetCQRSRequestPayload().SetResult(ExecutionResult.WithPayload(CommandResult.Success));
            return Task.CompletedTask;
        };

        var httpContext = await SendAsync();

        var executionResult = ExtractExecutionResult(httpContext);
        executionResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        var commandResult = ExtractCommandResult(executionResult);
        commandResult.WasSuccessful.Should().BeTrue();
    }

    private Task<HttpContext> SendAsync()
    {
        return server.SendAsync(ctx =>
        {
            var cqrsMetadata = new CQRSObjectMetadata(
                CQRSObjectKind.Command,
                typeof(Command),
                typeof(CommandResult),
                typeof(Ignore)
            );

            ctx.SetEndpoint(TestHelpers.MockCQRSEndpoint(cqrsMetadata));
            ctx.SetCQRSRequestPayload(new Command());
        });
    }

    private static ExecutionResult ExtractExecutionResult(HttpContext httpContext)
    {
        var cqrsPayload = httpContext.GetCQRSRequestPayload();

        cqrsPayload.Result.Should().NotBeNull();
        return cqrsPayload.Result!.Value;
    }

    private static CommandResult ExtractCommandResult(ExecutionResult result)
    {
        return result.Payload.Should().BeOfType<CommandResult>().Subject;
    }

    public Task InitializeAsync() => host.StartAsync();

    public Task DisposeAsync() => host.StopAsync();

    public void Dispose()
    {
        server.Dispose();
        host.Dispose();
    }

    private sealed class Command : ICommand { }

    private sealed class Ignore { }
}
