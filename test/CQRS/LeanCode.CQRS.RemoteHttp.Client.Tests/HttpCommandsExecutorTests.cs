using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LeanCode.Contracts;
using Xunit;

namespace LeanCode.CQRS.RemoteHttp.Client.Tests;

public class HttpCommandsExecutorTests
{
    [Fact]
    public async Task Correctly_builds_request_path()
    {
        var (exec, handler) = Prepare(HttpStatusCode.OK, "{}");

        await exec.RunAsync(new ExampleCommand());

        Assert.NotNull(handler.Request);
        Assert.Equal(
            new Uri("http://localhost/command/" + typeof(ExampleCommand).FullName),
            handler.Request!.RequestUri
        );
    }

    [Fact]
    public async Task Serializes_the_request_payload()
    {
        var (exec, handler) = Prepare(HttpStatusCode.OK, "{}");

        await exec.RunAsync(new ExampleCommand { RequestData = "data" });

        Assert.NotNull(handler.Request);
        var content = await handler!.Request!.Content!.ReadAsStringAsync();
        Assert.Equal("{\"RequestData\":\"data\"}", content);
    }

    [Fact]
    public async Task Correctly_deserializes_the_command_result_in_case_of_no_validation_errors()
    {
        var (exec, _) = Prepare(HttpStatusCode.OK, "{\"ValidationErrors\":[]}");

        var result = await exec.RunAsync(new ExampleCommand());

        Assert.NotNull(result);
        Assert.True(result.WasSuccessful);
    }

    [Fact]
    public async Task Correctly_deserializes_the_command_result_if_server_returns_errors_but_not_unprocessable_entity_code()
    {
        var (exec, _) = Prepare(HttpStatusCode.OK, "{\"ValidationErrors\":[{}]}");

        var result = await exec.RunAsync(new ExampleCommand());

        Assert.NotNull(result);
        Assert.True(result.WasSuccessful);
    }

    [Fact]
    public async Task Correctly_deserializes_validation_errors()
    {
        var (exec, _) = Prepare(
            HttpStatusCode.UnprocessableEntity,
            "{\"ValidationErrors\":[{\"ErrorCode\":1,\"ErrorMessage\":\"msg\",\"PropertyName\":\"prop\"}]}"
        );

        var result = await exec.RunAsync(new ExampleCommand());

        Assert.NotNull(result);
        Assert.False(result.WasSuccessful);
        var error = Assert.Single(result.ValidationErrors);
        Assert.Equal(1, error.ErrorCode);
        Assert.Equal("prop", error.PropertyName);
        Assert.Equal("msg", error.ErrorMessage);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA2000", Justification = "Can be ignored in tests.")]
    public async Task Correctly_deserializes_validation_errors_even_in_the_presence_of_custom_serialization_options()
    {
        var handler = new ShortcircuitingJsonHandler(
            HttpStatusCode.UnprocessableEntity,
            "{\"ValidationErrors\":[{\"ErrorCode\":1,\"ErrorMessage\":\"msg\",\"PropertyName\":\"prop\"}]}"
        );
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var exec = new HttpCommandsExecutor(client, new JsonSerializerOptions { AllowTrailingCommas = true });

        var result = await exec.RunAsync(new ExampleCommand());

        Assert.NotNull(result);
        Assert.False(result.WasSuccessful);
        var error = Assert.Single(result.ValidationErrors);
        Assert.Equal(1, error.ErrorCode);
        Assert.Equal("prop", error.PropertyName);
        Assert.Equal("msg", error.ErrorMessage);
    }

    [Fact]
    public async Task Throws_when_the_server_returns_null()
    {
        var (exec, _) = Prepare(HttpStatusCode.UnprocessableEntity, "null");

        await Assert.ThrowsAsync<MalformedResponseException>(() => exec.RunAsync(new ExampleCommand()));
    }

    [Fact]
    public async Task Handles_unauthorized()
    {
        await TestExceptionAsync<UnauthorizedException>(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Handles_forbidden()
    {
        await TestExceptionAsync<ForbiddenException>(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Handles_internal_server_errors()
    {
        await TestExceptionAsync<InternalServerErrorException>(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Handles_not_found()
    {
        await TestExceptionAsync<CommandNotFoundException>(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Handles_malformed()
    {
        await TestExceptionAsync<InvalidCommandException>(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Handles_other_errors()
    {
        await TestExceptionAsync<HttpCallErrorException>(HttpStatusCode.BadGateway);
    }

    [Fact]
    public async Task Wraps_JsonException_in_MalformedResponseException()
    {
        var (exec, _) = Prepare(HttpStatusCode.UnprocessableEntity, "[{\"");

        await Assert.ThrowsAsync<MalformedResponseException>(() => exec.RunAsync(new ExampleCommand()));
    }

    private static async Task TestExceptionAsync<TException>(HttpStatusCode statusCode)
        where TException : Exception
    {
        var (exec, _) = Prepare(statusCode, "");

        await Assert.ThrowsAsync<TException>(() => exec.RunAsync(new ExampleCommand()));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA2000",
        Justification = "References don't go out of scope."
    )]
    private static (HttpCommandsExecutor, ShortcircuitingJsonHandler) Prepare(HttpStatusCode statusCode, string result)
    {
        var handler = new ShortcircuitingJsonHandler(statusCode, result);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        return (new HttpCommandsExecutor(client), handler);
    }
}

public class ExampleCommand : ICommand
{
    public string? RequestData { get; set; }
}
