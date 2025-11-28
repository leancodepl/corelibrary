using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using LeanCode.Contracts;
using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.Execution;
using LeanCode.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Middleware;

[SuppressMessage(category: "?", "CA1034", Justification = "Nesting public types for better tests separation")]
public sealed class CQRSMiddlewareTests : CQRSMiddlewareTestBase<CQRSMiddleware>
{
    private static readonly QueryResult FinalResult = new() { Value = 42 };

    private static readonly CQRSObjectMetadata QueryMetadata = new(
        CQRSObjectKind.Query,
        typeof(Query),
        typeof(QueryResult),
        typeof(IgnoreType),
        (_, _) => Task.FromResult<object?>(FinalResult)
    );

    private Func<HttpContext, ExecutionResult?> IntermediatePipeline { get; set; } = _ => null;

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(logging => logging.AddNullLeanCodeLogger());
    }

    public CQRSMiddlewareTests()
    {
        FinalPipeline = ctx =>
            IntermediatePipeline(ctx) is { } result
                ? ctx.CompleteCQRSExecutionResult(result)
                : CQRSPipelineFinalizer.HandleAsync(ctx);
    }

    [Fact]
    public async Task Returns_400BadRequest_if_cannot_deserialize_request_body()
    {
        Serializer.SetDeserializeException<Query>(new InvalidOperationException("Failed to deserialize"));

        var httpContext = await SendAsync();

        httpContext.ShouldHaveResponseStatusCode(StatusCodes.Status400BadRequest);
        VerifyActivity(
            $"{QueryMetadata.ObjectKind} - {QueryMetadata.HandlerType.FullName}",
            activityStatusCode: ActivityStatusCode.Error,
            failureReason: CQRSMetrics.SerializationFailure,
            exceptionShouldBeRecorded: true
        );
        VerifyCQRSFailureMetrics(CQRSMetrics.SerializationFailure, 1);
    }

    [Fact]
    public async Task Returns_400BadRequest_if_deserialized_body_is_null()
    {
        Serializer.SetDeserializeResult<Query>(null);

        var httpContext = await SendAsync();

        httpContext.ShouldHaveResponseStatusCode(StatusCodes.Status400BadRequest);
        VerifyActivity(activityStatusCode: ActivityStatusCode.Error, failureReason: CQRSMetrics.SerializationFailure);
        VerifyCQRSFailureMetrics(CQRSMetrics.SerializationFailure, 1);
    }

    [Fact]
    public async Task Deserializes_request_then_passes_payload_to_further_pipeline_then_serializes_result()
    {
        var query = new Query();

        Serializer.SetDeserializeResult(query);

        var httpContext = await SendAsync();

        httpContext
            .ShouldHaveResponseStatusCode(StatusCodes.Status200OK)
            .ShouldHaveResponseContentType(Serializer.ContentType)
            .ShouldContainExecutionResult(StatusCodes.Status200OK, FinalResult);
        Serializer.ShouldHaveSerialized(FinalResult);
        VerifyActivity(activityStatusCode: ActivityStatusCode.Ok);
        VerifyCQRSSuccessMetrics(1);
    }

    [Fact]
    public async Task Allows_for_custom_result_code_for_execution_success()
    {
        var query = new Query();
        var queryResult = new QueryResult();

        Serializer.SetDeserializeResult(query);
        IntermediatePipeline = _ => ExecutionResult.WithPayload(queryResult, StatusCodes.Status202Accepted);

        var httpContext = await SendAsync();

        httpContext
            .ShouldHaveResponseStatusCode(StatusCodes.Status202Accepted)
            .ShouldHaveResponseContentType(Serializer.ContentType)
            .ShouldContainExecutionResult(StatusCodes.Status202Accepted, queryResult);

        Serializer.ShouldHaveSerialized(queryResult);

        VerifyActivity(activityStatusCode: ActivityStatusCode.Ok);
        VerifyCQRSSuccessMetrics(1);
    }

    [Fact]
    public async Task Returns_failure_code_when_intermediate_pipeline_returns_failure()
    {
        var query = new Query();

        Serializer.SetDeserializeResult(query);
        IntermediatePipeline = _ => ExecutionResult.Empty(StatusCodes.Status418ImATeapot);

        var httpContext = await SendAsync();

        httpContext
            .ShouldHaveResponseStatusCode(StatusCodes.Status418ImATeapot)
            .ShouldContainExecutionResult(StatusCodes.Status418ImATeapot);

        // Intermediate middlewares are responsible for serializing the result if there is any,
        // so we don't need to verify that.

        VerifyActivity();
        // Intermediate middlewares are responsible for collecting controlled failure metrics and logging,
        // so we don't need to verify that.
        VerifyNoCQRSSuccessMetrics();
        VerifyNoCQRSFailureMetrics();
    }

    [Fact]
    public async Task Enforces_serialized_response_type_according_to_cqrs_metadata()
    {
        var query = new Query();
        var queryResult = new QueryRuntimeResult();

        Serializer.SetDeserializeResult(query);
        IntermediatePipeline = _ => ExecutionResult.WithPayload(queryResult, StatusCodes.Status200OK);

        var httpContext = await SendAsync();

        httpContext
            .ShouldHaveResponseStatusCode(StatusCodes.Status200OK)
            .ShouldHaveResponseContentType(Serializer.ContentType)
            .ShouldContainExecutionResult(StatusCodes.Status200OK, queryResult);

        Serializer.ShouldHaveSerialized(queryResult, typeof(QueryResult));

        VerifyActivity(activityStatusCode: ActivityStatusCode.Ok);
        VerifyCQRSSuccessMetrics(1);
    }

    [Fact]
    public async Task Returns_500InternalServerError_if_pipeline_execution_thrown_an_exception()
    {
        var query = new Query();
        Serializer.SetDeserializeResult(query);

        IntermediatePipeline = _ => throw new InvalidOperationException();

        var httpContext = await SendAsync();

        httpContext.ShouldHaveResponseStatusCode(StatusCodes.Status500InternalServerError);
        VerifyActivity(
            activityStatusCode: ActivityStatusCode.Error,
            failureReason: CQRSMetrics.InternalError,
            exceptionShouldBeRecorded: true
        );
        VerifyCQRSFailureMetrics(CQRSMetrics.InternalError, 1);
    }

    [Fact]
    public async Task Correctly_passes_payload_and_context()
    {
        var query = new Query();
        Serializer.SetDeserializeResult(query);

        object interceptedPayload = null!;

        IntermediatePipeline = ctx =>
        {
            var payload = ctx.GetCQRSRequestPayload();
            interceptedPayload = payload.Payload;
            return null;
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

    private void VerifyActivity(
        ActivityStatusCode? activityStatusCode = null,
        string? failureReason = null,
        bool exceptionShouldBeRecorded = false
    )
    {
        VerifyActivity(
            $"{QueryMetadata.ObjectKind} - {QueryMetadata.HandlerType.FullName}",
            activityStatusCode,
            failureReason,
            exceptionShouldBeRecorded
        );
    }

    private sealed class Query : IQuery<QueryResult>;

    private class QueryResult
    {
        public int Value { get; set; }
    }

    private sealed class QueryRuntimeResult : QueryResult;

    private sealed class IgnoreType;
}
