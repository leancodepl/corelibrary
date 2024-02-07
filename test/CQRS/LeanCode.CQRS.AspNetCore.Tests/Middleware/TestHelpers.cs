using FluentAssertions;
using FluentAssertions.Execution;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Tests.Middleware;

public static class TestHelpers
{
    public static Endpoint MockCQRSEndpoint(CQRSObjectMetadata obj)
    {
        return new Endpoint(null, new EndpointMetadataCollection(obj), obj.ObjectType.Name);
    }

    public static ExecutionResult ShouldContainExecutionResult(this HttpContext httpContext, int statusCode)
    {
        var cqrsPayload = httpContext.GetCQRSRequestPayload();

        cqrsPayload.Result.Should().NotBeNull("because httpContext should contain cqrs execution result");
        var result = cqrsPayload.Result!.Value;
        result.StatusCode.Should().Be(statusCode);

        return result;
    }

    public static CommandResult ShouldContainCommandResult(this ExecutionResult executionResult)
    {
        return executionResult
            .Payload
            .Should()
            .BeOfType<CommandResult>("because execution result should be a command result")
            .Subject;
    }

    public static void ShouldBeSuccessful(this CommandResult commandResult)
    {
        using var _ = new AssertionScope();
        commandResult.ValidationErrors.Should().BeEmpty("command should not contain validation errors");
        commandResult.WasSuccessful.Should().BeTrue();
    }

    public static void ShouldFailWithValidationErrors(this CommandResult commandResult, params ValidationError[] errors)
    {
        using var _ = new AssertionScope();

        commandResult.WasSuccessful.Should().BeFalse();
        commandResult.ValidationErrors.Should().BeEquivalentTo(errors);
    }

    public static HttpContext ShouldHaveResponseStatusCode(this HttpContext context, int statusCode)
    {
        context.Response.StatusCode.Should().Be(statusCode);
        return context;
    }

    public static HttpContext ShouldHaveResponseContentType(this HttpContext context, string? contentType)
    {
        context.Response.ContentType.Should().Be(contentType);
        return context;
    }
}
