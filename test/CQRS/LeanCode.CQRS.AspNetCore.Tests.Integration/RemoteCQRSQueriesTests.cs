using System.Net;
using System.Text.Json;
using LeanCode.Contracts;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration;

public class RemoteCQRSQueriesTests : RemoteCQRSTestsBase
{
    [Fact]
    public async Task Returns_NotFound_for_non_existing_command()
    {
        var (_, statusCode) = await SendAsync("/cqrs/query/LeanCode.NotAValidQuery");
        Assert.Equal(HttpStatusCode.NotFound, statusCode);
    }

    [Fact]
    public async Task Returns_MethodNotAllowed_when_using_incorrect_verb_PUT()
    {
        var (_, statusCode) = await SendAsync(
            "/cqrs/query/LeanCode.CQRS.AspNetCore.Tests.Integration.TestQuery",
            method: HttpMethod.Put
        );

        Assert.Equal(HttpStatusCode.MethodNotAllowed, statusCode);
    }

    [Fact]
    public async Task Returns_MethodNotAllowed_when_using_incorrect_verb_GET()
    {
        var (_, statusCode) = await SendAsync(
            "/cqrs/query/LeanCode.CQRS.AspNetCore.Tests.Integration.TestQuery",
            method: HttpMethod.Get
        );

        Assert.Equal(HttpStatusCode.MethodNotAllowed, statusCode);
    }

    [Fact]
    public async Task Returns_OK_with_result_on_success()
    {
        var (body, statusCode) = await SendAsync(
            "/cqrs/query/LeanCode.CQRS.AspNetCore.Tests.Integration.TestQuery",
            @"{ ""X"": 2, ""Y"": 3 }"
        );

        Assert.Equal(HttpStatusCode.OK, statusCode);
        var result = JsonSerializer.Deserialize<TestQueryResult>(body);
        Assert.Equal(5, result.Sum);
    }

    [Fact]
    public async Task Returns_BadRequest_if_failed_to_deserialize_object()
    {
        var (_, statusCode) = await SendAsync(
            "/cqrs/query/LeanCode.CQRS.AspNetCore.Tests.Integration.TestQuery",
            "{ malformed json }"
        );
        Assert.Equal(HttpStatusCode.BadRequest, statusCode);
    }

    [Fact]
    public async Task Returns_Unauthorized_for_when_user_is_not_authenticated()
    {
        var (_, statusCode) = await SendAsync(
            "/cqrs/query/LeanCode.CQRS.AspNetCore.Tests.Integration.TestQuery",
            isAuthenticated: false
        );

        Assert.Equal(HttpStatusCode.Unauthorized, statusCode);
    }

    [Fact]
    public async Task Returns_Forbidden_if_authorizer_fails()
    {
        var (_, statusCode) = await SendAsync(
            "/cqrs/query/LeanCode.CQRS.AspNetCore.Tests.Integration.TestQuery",
            @"{ ""FailAuthorization"": true }"
        );

        Assert.Equal(HttpStatusCode.Forbidden, statusCode);
    }
}
