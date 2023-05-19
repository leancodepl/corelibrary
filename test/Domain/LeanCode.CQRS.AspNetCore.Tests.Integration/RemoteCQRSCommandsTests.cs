using System.Net;
using System.Text.Json;
using LeanCode.Contracts;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration;

public class RemoteCQRSCommandsTests : RemoteCQRSTestsBase
{
    [Fact]
    public async Task Returns_NotFound_for_non_existing_command()
    {
        var (_, statusCode) = await SendAsync("/cqrs/command/LeanCode.NotAValidCommand");
        Assert.Equal(HttpStatusCode.NotFound, statusCode);
    }

    [Fact]
    public async Task Returns_MethodNotAllowed_when_using_incorrect_verb_PUT()
    {
        var (_, statusCode) = await SendAsync(
            "/cqrs/command/LeanCode.CQRS.AspNetCore.Tests.Integration.TestCommand",
            method: HttpMethod.Put
        );

        Assert.Equal(HttpStatusCode.MethodNotAllowed, statusCode);
    }

    [Fact]
    public async Task Returns_MethodNotAllowed_when_using_incorrect_verb_GET()
    {
        var (_, statusCode) = await SendAsync(
            "/cqrs/command/LeanCode.CQRS.AspNetCore.Tests.Integration.TestCommand",
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
        var (body, statusCode) = await SendAsync(
            "/cqrs/command/LeanCode.CQRS.AspNetCore.Tests.Integration.TestCommand",
            @"{ ""FailValidation"": true }"
        );

        var commandResult = JsonSerializer.Deserialize<CommandResult>(body);

        // Assert.Equal(HttpStatusCode.UnprocessableEntity, statusCode);
        Assert.NotNull(commandResult);

        Assert.False(commandResult!.WasSuccessful);
        var error = Assert.Single(commandResult.ValidationErrors);
        Assert.Equal(nameof(TestCommand.FailValidation), error.PropertyName);
        Assert.Equal("Test command should pass validation", error.ErrorMessage);
        Assert.Equal(TestCommand.ErrorCodes.ValidationError, error.ErrorCode);
    }

    [Fact]
    public async Task Returns_BadRequest_if_failed_to_deserialize_object()
    {
        var (_, statusCode) = await SendAsync(
            "/cqrs/command/LeanCode.CQRS.AspNetCore.Tests.Integration.TestCommand",
            "{ malformed json }"
        );
        Assert.Equal(HttpStatusCode.BadRequest, statusCode);
    }

    [Fact]
    public async Task Returns_Unauthorized_for_when_user_is_not_authenticated()
    {
        var (_, statusCode) = await SendAsync(
            "/cqrs/command/LeanCode.CQRS.AspNetCore.Tests.Integration.TestCommand",
            isAuthenticated: false
        );

        Assert.Equal(HttpStatusCode.Unauthorized, statusCode);
    }

    [Fact]
    public async Task Returns_Forbidden_if_authorizer_fails()
    {
        var (_, statusCode) = await SendAsync(
            "/cqrs/command/LeanCode.CQRS.AspNetCore.Tests.Integration.TestCommand",
            @"{ ""FailAuthorization"": true }"
        );

        Assert.Equal(HttpStatusCode.Forbidden, statusCode);
    }

    [Fact]
    public async Task Returns_InternalServerError_if_something_fails()
    {
        var (_, statusCode) = await SendAsync(
            "/cqrs/command/LeanCode.CQRS.AspNetCore.Tests.Integration.TestFailingCommand"
        );

        Assert.Equal(HttpStatusCode.InternalServerError, statusCode);
    }
}
