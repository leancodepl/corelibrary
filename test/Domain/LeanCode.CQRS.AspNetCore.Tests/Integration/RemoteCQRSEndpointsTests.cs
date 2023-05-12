using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using LeanCode.Components;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using LeanCode.CQRS.AspNetCore.Registration;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration;

public class RemoteCQRSEndpointsTests : IDisposable, IAsyncLifetime
{
    private readonly TypesCatalog contractsCatalog = TypesCatalog.Of<TestContext>();
    private readonly TypesCatalog handlersCatalog = TypesCatalog.Of<TestCommandHandler>();

    private readonly IHost host;

    public RemoteCQRSEndpointsTests()
    {
        host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddRouting();
                        services.AddCQRS(handlersCatalog);
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(e =>
                        {
                            e.MapRemoteCqrs("/cqrs", cqrs => cqrs
                                .WithPipelines(contractsCatalog, TestContext.FromHttp, q => { }, c => { }, o => { })
                            );
                        });
                    });
            })
            .Build();
    }

    [Fact]
    public async Task Returns_NotFound_for_non_existing_command()
    {
        var (_, statusCode) = await SendAsync("/cqrs/commands/LeanCode.NotAValidCommand");

        Assert.Equal(HttpStatusCode.NotFound, statusCode);
    }

    [Fact]
    public async Task Returns_MethodNotAllowed_when_using_incorrect_verb_PUT()
    {
        var (_, statusCode) = await SendAsync(
            "/cqrs/command/LeanCode.CQRS.RemoteHttp.Server.Tests.SampleRemoteCommand",
            method: HttpMethod.Put
        );

        Assert.Equal(HttpStatusCode.MethodNotAllowed, statusCode);
    }

    [Fact]
    public async Task Returns_MethodNotAllowed_when_using_incorrect_verb_GET()
    {
        var (_, statusCode) = await SendAsync(
            "/cqrs/command/LeanCode.CQRS.RemoteHttp.Server.Tests.SampleRemoteCommand",
            method: HttpMethod.Get
        );

        Assert.Equal(HttpStatusCode.MethodNotAllowed, statusCode);
    }

    [Fact]
    public async Task Returns_OK_with_successful_command_result_on_success()
    {
        var (body, statusCode) = await SendAsync(
            "/cqrs/command/LeanCode.CQRS.AspNetCore.Tests.Integration.TestCommand"
        );

        Assert.Equal(HttpStatusCode.OK, statusCode);
        var result = JsonSerializer.Deserialize<CommandResult?>(body);
        Assert.NotNull(result);
        Assert.True(result!.WasSuccessful);
    }

    [Fact]
    public async Task Returns_UnprocessableEntity_with_errors_when_validation_fails()
    {
        var returnedErrors = new ValidationError[]
        {
            new("Property1", "ErrorMessage1", 1),
            new("Property2", "ErrorMessage2", 2),
        };

        // command
        //     .RunAsync(Arg.Any<AppContext>(), Arg.Is<ICommand>(x => x is SampleRemoteCommand))
        //     .Returns(CommandResult.NotValid(new ValidationResult(returnedErrors)));

        var (body, statusCode) = await SendAsync(
            "/cqrs/command/LeanCode.CQRS.RemoteHttp.Server.Tests.SampleRemoteCommand"
        );
        var result = JsonSerializer.Deserialize<CommandResult?>(body);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, statusCode);
        Assert.NotNull(result);
        Assert.False(result!.WasSuccessful);
        Assert.Collection(
            result.ValidationErrors,
            e1 =>
            {
                Assert.Equal("Property1", e1.PropertyName);
                Assert.Equal("ErrorMessage1", e1.ErrorMessage);
                Assert.Equal(1, e1.ErrorCode);
            },
            e2 =>
            {
                Assert.Equal("Property2", e2.PropertyName);
                Assert.Equal("ErrorMessage2", e2.ErrorMessage);
                Assert.Equal(2, e2.ErrorCode);
            }
        );
    }

    [Fact]
    public async Task Returns_BadRequest_if_failed_to_deserialize_object()
    {
        var (_, statusCode) = await SendAsync(
            "/cqrs/command/LeanCode.CQRS.RemoteHttp.Server.Tests.SampleRemoteCommand",
            "{ malformed json }"
        );
        Assert.Equal(HttpStatusCode.BadRequest, statusCode);
    }

    // [Fact]
    // public async Task Returns_Unauthorized_for_UnauthenticatedException()
    // {
    //     command
    //         .RunAsync(Arg.Any<AppContext>(), Arg.Is<ICommand>(x => x is SampleRemoteCommand))
    //         .Throws(new UnauthenticatedException("User not authenticated"));
    //
    //     var (_, statusCode) = await SendAsync(
    //         "/cqrs/command/LeanCode.CQRS.RemoteHttp.Server.Tests.SampleRemoteCommand"
    //     );
    //     Assert.Equal(HttpStatusCode.Unauthorized, statusCode);
    // }
    //
    // [Fact]
    // public async Task Returns_Forbidden_for_InsufficientPermissionException()
    // {
    //     command
    //         .RunAsync(Arg.Any<AppContext>(), Arg.Is<ICommand>(x => x is SampleRemoteCommand))
    //         .Throws(new InsufficientPermissionException("User not authorized", null));
    //
    //     var (_, statusCode) = await SendAsync(
    //         "/cqrs/command/LeanCode.CQRS.RemoteHttp.Server.Tests.SampleRemoteCommand"
    //     );
    //     Assert.Equal(HttpStatusCode.Forbidden, statusCode);
    // }

    private async Task<(string ReponseBody, HttpStatusCode StatusCode)> SendAsync(
        string path,
        string body = "{}",
        HttpMethod? method = null
    )
    {
        method ??= HttpMethod.Post;

        using var msg = new HttpRequestMessage(method, path);
        if (method == HttpMethod.Post)
        {
            msg.Content = new StringContent(body, new MediaTypeHeaderValue("application/json", "utf-8"));
        }

        var response = await host.GetTestClient().SendAsync(msg);

        var responseBody = await response.Content.ReadAsStringAsync();
        return (responseBody, response.StatusCode);
    }

    public void Dispose()
    {
        host.Dispose();
    }

    public Task InitializeAsync()
    {
        return host.StartAsync();
    }

    public Task DisposeAsync()
    {
        return host.StopAsync();
    }
}
