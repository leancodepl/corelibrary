using LeanCode.Contracts;
using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.AspNetCore.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Middleware;

public class CQRSRequestSerializerTests : IDisposable, IAsyncLifetime
{
    private static readonly CQRSObjectMetadata QueryMetadata =
        new(CQRSObjectKind.Query, typeof(Query), typeof(QueryResult), typeof(IgnoreType));

    private readonly IHost host;
    private readonly TestServer server;
    private readonly ISerializer serializer;
    private RequestDelegate pipeline;

    public CQRSRequestSerializerTests()
    {
        serializer = Substitute.For<ISerializer>();
        pipeline = ctx => Task.CompletedTask;

        host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(cfg =>
                    {
                        cfg.AddSingleton(serializer);
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<CQRSRequestSerializer>();
                        app.Run(ctx => pipeline(ctx));
                    });
            })
            .Build();

        server = host.GetTestServer();
    }

    [Fact]
    public async Task Returns_400BadRequest_if_cannot_deserialize_request_body()
    {
        SetDeserializerResult<Query>(new InvalidOperationException("Failed to deserialize"));

        var httpContext = await SendAsync();

        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Returns_400BadRequest_if_deserialized_body_is_null()
    {
        SetDeserializerResult<Query>(null as Query);

        var httpContext = await SendAsync();

        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Deserializes_request_then_passes_payload_to_further_pipeline_then_serializes_result()
    {
        var query = new Query();
        var queryResult = new QueryResult();

        SetDeserializerResult<Query>(query);
        SetPipelineResultSuccess(queryResult);

        var httpContext = await SendAsync();

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
        Assert.Equal("application/json", httpContext.Response.ContentType);
        await serializer
            .Received()
            .SerializeAsync(Arg.Any<Stream>(), queryResult, typeof(QueryResult), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Allows_for_custom_result_code_for_execution_success()
    {
        var query = new Query();
        var queryResult = new QueryResult();

        SetDeserializerResult<Query>(query);
        SetPipelineResultSuccess(queryResult, StatusCodes.Status202Accepted);

        var httpContext = await SendAsync();

        Assert.Equal(StatusCodes.Status202Accepted, httpContext.Response.StatusCode);
        Assert.Equal("application/json", httpContext.Response.ContentType);
        await serializer
            .Received()
            .SerializeAsync(Arg.Any<Stream>(), queryResult, typeof(QueryResult), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_failure_code_and_does_not_serialize_result_for_failure()
    {
        var query = new Query();

        SetDeserializerResult<Query>(query);
        SetPipelineResultFailure(StatusCodes.Status418ImATeapot);

        var httpContext = await SendAsync();

        Assert.Equal(StatusCodes.Status418ImATeapot, httpContext.Response.StatusCode);
        Assert.Null(httpContext.Response.ContentType);
        await serializer.DidNotReceiveWithAnyArgs().SerializeAsync(default!, default!, default!, default!);
    }

    [Fact]
    public async Task Enforces_serialized_response_type_according_to_cqrs_metadata()
    {
        var query = new Query();
        var queryResult = new QueryRuntimeResult();

        SetDeserializerResult<Query>(query);
        SetPipelineResultSuccess(queryResult);

        var httpContext = await SendAsync();

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
        Assert.Equal("application/json", httpContext.Response.ContentType);
        await serializer
            .Received()
            .SerializeAsync(Arg.Any<Stream>(), queryResult, typeof(QueryResult), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Skips_serializing_result_if_it_was_not_set()
    {
        var query = new Query();
        SetDeserializerResult<Query>(query);

        await SendAsync();
        await serializer.DidNotReceiveWithAnyArgs().SerializeAsync(null!, null!, null!, default);
    }

    [Fact]
    public async Task Returns_500InternalServerError_if_pipeline_execution_thrown_an_exception()
    {
        var query = new Query();
        SetDeserializerResult<Query>(query);

        pipeline = ctx => throw new InvalidOperationException();

        var httpContext = await SendAsync();

        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Correctly_passes_payload_and_context()
    {
        var query = new Query();
        SetDeserializerResult<Query>(query);

        object? interceptedPayload = null!;

        pipeline = ctx =>
        {
            var payload = ctx.GetCQRSRequestPayload();
            interceptedPayload = payload.Payload;
            return Task.CompletedTask;
        };

        await SendAsync();

        Assert.Equal(query, interceptedPayload);
    }

    private Task<HttpContext> SendAsync(Action<HttpContext>? config = null)
    {
        return server.SendAsync(ctx =>
        {
            config?.Invoke(ctx);

            var endpointMetadata = new CQRSEndpointMetadata(QueryMetadata, (_, __) => Task.FromResult<object?>(null));
            var endpoint = new Endpoint(null, new EndpointMetadataCollection(endpointMetadata), "Test endpoint");
            ctx.SetEndpoint(endpoint);
        });
    }

    private void SetDeserializerResult<TResult>(object? obj)
    {
        serializer.DeserializeAsync(Arg.Any<Stream>(), typeof(TResult), Arg.Any<CancellationToken>()).Returns(obj);
    }

    private void SetDeserializerResult<TResult>(Exception ex)
    {
        // Silence CA2012: Use ValueTasks correctly
        var result = serializer.DeserializeAsync(Arg.Any<Stream>(), typeof(TResult), Arg.Any<CancellationToken>());
        result.Throws(ex);
    }

    private void SetPipelineResultSuccess(object? obj, int code = 200)
    {
        pipeline = ctx =>
        {
            var payload = ctx.GetCQRSRequestPayload();
            payload.SetResult(ExecutionResult.Success(obj, code));
            return Task.CompletedTask;
        };
    }

    private void SetPipelineResultFailure(int code)
    {
        pipeline = ctx =>
        {
            var payload = ctx.GetCQRSRequestPayload();
            payload.SetResult(ExecutionResult.Fail(code));
            return Task.CompletedTask;
        };
    }

    private class Query : IQuery<QueryResult> { }

    private class QueryResult { }

    private class QueryRuntimeResult : QueryResult { }

    private class IgnoreType { }

    public void Dispose()
    {
        server.Dispose();
        host.Dispose();
    }

    public Task InitializeAsync() => host.StartAsync();

    public Task DisposeAsync() => host.StopAsync();
}
