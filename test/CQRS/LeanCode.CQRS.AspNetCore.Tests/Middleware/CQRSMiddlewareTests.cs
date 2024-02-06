using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using LeanCode.Contracts;
using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.AspNetCore.Serialization;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Middleware;

[SuppressMessage(category: "?", "CA1034", Justification = "Nesting public types for better tests separation")]
public sealed class CQRSMiddlewareTests : CQRSMiddlewareTestBase<CQRSMiddleware>
{
    private static readonly CQRSObjectMetadata QueryMetadata =
        new(
            CQRSObjectKind.Query,
            typeof(Query),
            typeof(QueryResult),
            typeof(IgnoreType),
            (_, _) => Task.FromResult<object?>(null)
        );

    private readonly ISerializer serializer = Substitute.For<ISerializer>();

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(_ => serializer);
    }

    [Fact]
    public async Task Returns_400BadRequest_if_cannot_deserialize_request_body()
    {
        SetDeserializerResult<Query>(new InvalidOperationException("Failed to deserialize"));

        var httpContext = await SendAsync();

        httpContext.ShouldHaveResponseStatusCode(StatusCodes.Status400BadRequest);
        VerifyCQRSFailureMetrics(CQRSMetrics.SerializationFailure, 1);
    }

    [Fact]
    public async Task Returns_400BadRequest_if_deserialized_body_is_null()
    {
        SetDeserializerResult<Query>(null as Query);

        var httpContext = await SendAsync();

        httpContext.ShouldHaveResponseStatusCode(StatusCodes.Status400BadRequest);
        VerifyCQRSFailureMetrics(CQRSMetrics.SerializationFailure, 1);
    }

    [Fact]
    public async Task Deserializes_request_then_passes_payload_to_further_pipeline_then_serializes_result()
    {
        var query = new Query();
        var queryResult = new QueryResult();

        SetDeserializerResult<Query>(query);
        SetPipelineResultSuccess(queryResult);

        var httpContext = await SendAsync();

        httpContext
            .ShouldHaveResponseStatusCode(StatusCodes.Status200OK)
            .ShouldHaveResponseContentType("application/json");
        await serializer
            .Received()
            .SerializeAsync(Arg.Any<Stream>(), queryResult, typeof(QueryResult), Arg.Any<CancellationToken>());
        VerifyCQRSSuccessMetrics(1);
    }

    [Fact]
    public async Task Allows_for_custom_result_code_for_execution_success()
    {
        var query = new Query();
        var queryResult = new QueryResult();

        SetDeserializerResult<Query>(query);
        SetPipelineResultSuccess(queryResult, StatusCodes.Status202Accepted);

        var httpContext = await SendAsync();

        httpContext
            .ShouldHaveResponseStatusCode(StatusCodes.Status202Accepted)
            .ShouldHaveResponseContentType("application/json");

        await serializer
            .Received()
            .SerializeAsync(Arg.Any<Stream>(), queryResult, typeof(QueryResult), Arg.Any<CancellationToken>());

        VerifyCQRSSuccessMetrics(1);
    }

    [Fact]
    public async Task Returns_failure_code_and_does_not_serialize_result_for_failure()
    {
        var query = new Query();

        SetDeserializerResult<Query>(query);
        SetPipelineResultFailure(StatusCodes.Status418ImATeapot);

        var httpContext = await SendAsync();

        httpContext.ShouldHaveResponseStatusCode(StatusCodes.Status418ImATeapot).ShouldHaveResponseContentType(null);

        await serializer.DidNotReceiveWithAnyArgs().SerializeAsync(default!, default!, default!, default!);

        VerifyCQRSFailureMetrics(CQRSMetrics.InternalError, 1);
    }

    [Fact]
    public async Task Enforces_serialized_response_type_according_to_cqrs_metadata()
    {
        var query = new Query();
        var queryResult = new QueryRuntimeResult();

        SetDeserializerResult<Query>(query);
        SetPipelineResultSuccess(queryResult);

        var httpContext = await SendAsync();

        httpContext
            .ShouldHaveResponseStatusCode(StatusCodes.Status200OK)
            .ShouldHaveResponseContentType("application/json");

        await serializer
            .Received()
            .SerializeAsync(Arg.Any<Stream>(), queryResult, typeof(QueryResult), Arg.Any<CancellationToken>());

        VerifyCQRSSuccessMetrics(1);
    }

    [Fact]
    public async Task Skips_serializing_result_if_it_was_not_set()
    {
        var query = new Query();
        SetDeserializerResult<Query>(query);

        await SendAsync();
        await serializer.DidNotReceiveWithAnyArgs().SerializeAsync(null!, null!, null!, default);
        VerifyCQRSFailureMetrics(CQRSMetrics.InternalError, 1);
    }

    [Fact]
    public async Task Returns_500InternalServerError_if_pipeline_execution_thrown_an_exception()
    {
        var query = new Query();
        SetDeserializerResult<Query>(query);

        FinalPipeline = ctx => throw new InvalidOperationException();

        var httpContext = await SendAsync();

        httpContext.ShouldHaveResponseStatusCode(StatusCodes.Status500InternalServerError);
        VerifyCQRSFailureMetrics(CQRSMetrics.InternalError, 1);
    }

    [Fact]
    public async Task Correctly_passes_payload_and_context()
    {
        var query = new Query();
        SetDeserializerResult<Query>(query);

        object? interceptedPayload = null!;

        FinalPipeline = ctx =>
        {
            var payload = ctx.GetCQRSRequestPayload();
            interceptedPayload = payload.Payload;
            return Task.CompletedTask;
        };

        await SendAsync();
        interceptedPayload.Should().Be(query);
    }

    private Task<HttpContext> SendAsync(Action<HttpContext>? config = null)
    {
        return Server.SendAsync(ctx =>
        {
            config?.Invoke(ctx);
            ctx.SetEndpoint(TestHelpers.MockCQRSEndpoint(QueryMetadata));
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
        FinalPipeline = ctx =>
        {
            var payload = ctx.GetCQRSRequestPayload();
            payload.SetResult(ExecutionResult.WithPayload(obj, code));
            return Task.CompletedTask;
        };
    }

    private void SetPipelineResultFailure(int code)
    {
        FinalPipeline = ctx =>
        {
            var payload = ctx.GetCQRSRequestPayload();
            payload.SetResult(ExecutionResult.Empty(code));
            return Task.CompletedTask;
        };
    }

    public class Query : IQuery<QueryResult> { }

    public class QueryResult { }

    public class QueryRuntimeResult : QueryResult { }

    public class IgnoreType { }
}
