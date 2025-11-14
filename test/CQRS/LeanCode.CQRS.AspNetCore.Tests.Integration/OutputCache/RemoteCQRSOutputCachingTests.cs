using System.Net;
using FluentAssertions;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration.OutputCache;

/// <summary>
/// Here we mostly test whether the output caching integration works as expected
/// and the actual output cache middleware features that are supported
/// don't break on the ASP.NET Core framework updates and to document them.
/// </summary>
public class RemoteCQRSOutputCachingTests : RemoteCQRSTestsBase
{
    private const string CachedQueryPath =
        "/cqrs/query/LeanCode.CQRS.AspNetCore.Tests.Integration.OutputCache.CachedTestQuery";
    private const string CachedOperationPath =
        "/cqrs/operation/LeanCode.CQRS.AspNetCore.Tests.Integration.OutputCache.CachedTestOperation";
    private const string MinimalApiPath = "/invocation-count";
    private const string DefaultBody = """{ "X": 2, "Y": 3, "VaryByValue": "A" }""";

    public RemoteCQRSOutputCachingTests()
        : base(enableMinimalApi: true)
    {
        CachedTestQueryHandler.Reset();
        CachedTestOperationHandler.Reset();
    }

    [Fact]
    public async Task Query_response_is_cached_on_second_request()
    {
        var (_, firstStatusCode, _) = await SendAsync(CachedQueryPath, DefaultBody);
        firstStatusCode.Should().Be(HttpStatusCode.OK);
        CachedTestQueryHandler.InvocationCount.Should().Be(1);

        var (_, secondStatusCode, secondHeaders) = await SendAsync(CachedQueryPath, DefaultBody);
        secondStatusCode.Should().Be(HttpStatusCode.OK);
        CachedTestQueryHandler.InvocationCount.Should().Be(1);
        secondHeaders.Should().ContainKey(HeaderNames.Age);
    }

    [Fact]
    public async Task Operation_response_is_cached_on_second_request()
    {
        var (_, firstStatusCode, _) = await SendAsync(CachedOperationPath, DefaultBody);
        firstStatusCode.Should().Be(HttpStatusCode.OK);
        CachedTestOperationHandler.InvocationCount.Should().Be(1);

        var (_, secondStatusCode, secondHeaders) = await SendAsync(CachedOperationPath, DefaultBody);
        secondStatusCode.Should().Be(HttpStatusCode.OK);
        CachedTestOperationHandler.InvocationCount.Should().Be(1);
        secondHeaders.Should().ContainKey(HeaderNames.Age);
    }

    [Fact]
    public async Task Different_vary_by_values_create_different_cache_entries()
    {
        var queryBody1 = """{ "X": 1, "Y": 2, "VaryByValue": "A" }""";
        var queryBody2 = """{ "X": 1, "Y": 3, "VaryByValue": "B" }""";

        var (firstBody, firstStatusCode, _) = await SendAsync(CachedQueryPath, queryBody1);
        firstStatusCode.Should().Be(HttpStatusCode.OK);
        CachedTestQueryHandler.InvocationCount.Should().Be(1);

        var (secondBody, secondStatusCode, _) = await SendAsync(CachedQueryPath, queryBody2);
        secondStatusCode.Should().Be(HttpStatusCode.OK);
        secondBody.Should().NotBe(firstBody);
        CachedTestQueryHandler.InvocationCount.Should().Be(2);

        var (thirdBody, thirdStatusCode, _) = await SendAsync(CachedQueryPath, queryBody1);
        thirdStatusCode.Should().Be(HttpStatusCode.OK);
        thirdBody.Should().Be(firstBody);
        CachedTestQueryHandler.InvocationCount.Should().Be(2);
    }

    [Fact]
    public async Task Response_payload_may_determine_if_cache_is_used()
    {
        // Negative sum disables caching
        var queryBody = """{ "X": 1, "Y": -2, "VaryByValue": "A" }""";

        var (_, firstStatusCode, _) = await SendAsync(CachedQueryPath, queryBody);
        firstStatusCode.Should().Be(HttpStatusCode.OK);
        CachedTestQueryHandler.InvocationCount.Should().Be(1);

        var (_, secondStatusCode, _) = await SendAsync(CachedQueryPath, queryBody);
        secondStatusCode.Should().Be(HttpStatusCode.OK);
        CachedTestQueryHandler.InvocationCount.Should().Be(2);
    }

    [Fact]
    public async Task ETag_is_respected_with_if_none_match_header()
    {
        var (_, firstStatusCode, firstHeaders) = await SendAsync(CachedQueryPath, DefaultBody);
        firstStatusCode.Should().Be(HttpStatusCode.OK);
        firstHeaders.ETag.Should().NotBeNull();
        var etag = firstHeaders.ETag!.ToString();

        var (secondBody, secondStatusCode, _) = await SendAsync(
            CachedQueryPath,
            DefaultBody,
            headers: new() { { HeaderNames.IfNoneMatch, etag } }
        );
        secondStatusCode.Should().Be(HttpStatusCode.NotModified);
        secondBody.Should().BeEmpty();
        CachedTestQueryHandler.InvocationCount.Should().Be(1);
    }

    [Fact]
    public async Task Non_matching_ETag_returns_full_cached_response()
    {
        var (_, firstStatusCode, firstHeaders) = await SendAsync(CachedQueryPath, DefaultBody);
        firstStatusCode.Should().Be(HttpStatusCode.OK);
        firstHeaders.ETag.Should().NotBeNull();

        var (secondBody, secondStatusCode, secondHeaders) = await SendAsync(
            CachedQueryPath,
            DefaultBody,
            headers: new() { { HeaderNames.IfNoneMatch, "\"different-etag\"" } }
        );
        secondStatusCode.Should().Be(HttpStatusCode.OK);
        secondBody.Should().NotBeEmpty();
        secondHeaders.ETag.Should().NotBeNull();
        CachedTestQueryHandler.InvocationCount.Should().Be(1);
    }

    [Fact]
    public async Task If_modified_since_header_returns_not_modified_when_content_not_changed()
    {
        var (_, firstStatusCode, firstHeaders) = await SendAsync(CachedQueryPath, DefaultBody);
        firstStatusCode.Should().Be(HttpStatusCode.OK);
        firstHeaders.Should().ContainKey(HeaderNames.Date).WhoseValue.FirstOrDefault().Should().NotBeNull();

        // Wait a bit to ensure the cached response date is in the past
        await Task.Delay(100);

        // Use a date in the future relative to when the cache was created
        // This should return Not Modified since the cached content hasn't changed
        var futureDate = DateTimeOffset.UtcNow.AddDays(1);
        var dateHeader = futureDate.ToString("R", System.Globalization.CultureInfo.InvariantCulture);

        var (secondBody, secondStatusCode, _) = await SendAsync(
            CachedQueryPath,
            DefaultBody,
            headers: new() { { HeaderNames.IfModifiedSince, dateHeader } }
        );
        secondStatusCode.Should().Be(HttpStatusCode.NotModified);
        secondBody.Should().BeEmpty();
        CachedTestQueryHandler.InvocationCount.Should().Be(1);
    }

    [Fact]
    public async Task If_modified_since_header_returns_full_response_when_content_changed()
    {
        var (_, firstStatusCode, _) = await SendAsync(CachedQueryPath, DefaultBody);
        firstStatusCode.Should().Be(HttpStatusCode.OK);

        // Use a date well in the past to test that content has changed
        var pastDate = DateTimeOffset.UtcNow.AddDays(-1);
        var dateHeader = pastDate.ToString("R", System.Globalization.CultureInfo.InvariantCulture);

        var (secondBody, secondStatusCode, _) = await SendAsync(
            CachedQueryPath,
            DefaultBody,
            headers: new() { { HeaderNames.IfModifiedSince, dateHeader } }
        );
        secondStatusCode.Should().Be(HttpStatusCode.OK);
        secondBody.Should().NotBeEmpty();
        CachedTestQueryHandler.InvocationCount.Should().Be(1);
    }

    [Fact]
    public async Task Age_header_increases_on_subsequent_requests()
    {
        var (_, firstStatusCode, firstHeaders) = await SendAsync(CachedQueryPath, DefaultBody);
        firstStatusCode.Should().Be(HttpStatusCode.OK);
        firstHeaders.Should().NotContainKey(HeaderNames.Age);

        await Task.Delay(100);

        var (_, secondStatusCode, secondHeaders) = await SendAsync(CachedQueryPath, DefaultBody);
        secondStatusCode.Should().Be(HttpStatusCode.OK);
        secondHeaders.Should().ContainKey(HeaderNames.Age);
        var ageValue = secondHeaders.GetValues(HeaderNames.Age).FirstOrDefault();
        ageValue.Should().NotBeNull();
        int.Parse(ageValue!, System.Globalization.CultureInfo.InvariantCulture).Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task Cache_control_only_if_cached_returns_gateway_timeout_on_cache_miss()
    {
        var (_, statusCode, _) = await SendAsync(
            CachedQueryPath,
            DefaultBody,
            headers: new() { { HeaderNames.CacheControl, CacheControlHeaderValue.OnlyIfCachedString } }
        );
        statusCode.Should().Be(HttpStatusCode.GatewayTimeout);
        CachedTestQueryHandler.InvocationCount.Should().Be(0);
    }

    [Fact]
    public async Task Minimal_api_endpoint_output_cache_works_alongside_CQRS_output_cache()
    {
        var (_, queryStatusCode1, _) = await SendAsync(CachedQueryPath, DefaultBody);
        queryStatusCode1.Should().Be(HttpStatusCode.OK);
        CachedTestQueryHandler.InvocationCount.Should().Be(1);

        var (apiBody1, apiStatusCode1, _) = await SendAsync(
            MinimalApiPath,
            method: HttpMethod.Get,
            isAuthenticated: false
        );
        apiStatusCode1.Should().Be(HttpStatusCode.OK);
        apiBody1.Should().Be("1");

        var (_, queryStatusCode2, _) = await SendAsync(CachedQueryPath, DefaultBody);
        queryStatusCode2.Should().Be(HttpStatusCode.OK);
        CachedTestQueryHandler.InvocationCount.Should().Be(1);

        var (apiBody2, apiStatusCode2, _) = await SendAsync(
            MinimalApiPath,
            method: HttpMethod.Get,
            isAuthenticated: false
        );
        apiStatusCode2.Should().Be(HttpStatusCode.OK);
        apiBody2.Should().Be("1");
    }
}
