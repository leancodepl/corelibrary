using FluentAssertions;
using LeanCode.CQRS.AspNetCore.Local.Context;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Local.Context;

public class NullHttpResponseTests
{
    private readonly NullHttpResponse response = new(Substitute.For<HttpContext>());

    [Fact]
    public void Headers_are_NullHeaderDictionary()
    {
        response.Headers.Should().BeSameAs(NullHeaderDictionary.Empty);
    }

    [Fact]
    public void Cookies_are_NullResponseCookies()
    {
        response.Cookies.Should().BeSameAs(NullResponseCookies.Empty);
    }

    [Fact]
    public void HasStarted_should_always_be_false()
    {
        response.HasStarted.Should().BeFalse();
    }

    [Fact]
    public void ContentLength_should_always_be_null()
    {
        response.ContentLength.Should().BeNull();

        response.ContentLength = 123;

        response.ContentLength.Should().BeNull();
    }

    [Fact]
    public void StatusCode_should_always_be_zero()
    {
        response.StatusCode.Should().Be(0);

        response.StatusCode = 123;

        response.StatusCode.Should().Be(0);
    }

    [Fact]
    public void ContentType_should_always_be_null()
    {
        response.ContentType.Should().BeNull();

        response.ContentType = "text/plain";

        response.ContentType.Should().BeNull();
    }

    [Fact]
    public void Body_should_alaways_be_a_null_stream()
    {
        response.Body.Should().BeSameAs(Stream.Null);

        response.Body = Substitute.For<Stream>();

        response.Body.Should().BeSameAs(Stream.Null);
    }

    [Fact]
    public void OnCompleted_does_nothing()
    {
        response.Invoking(r => r.OnCompleted(_ => Task.CompletedTask, new())).Should().NotThrow();
    }

    [Fact]
    public void OnStarting_does_nothing()
    {
        response.Invoking(r => r.OnStarting(_ => Task.CompletedTask, new())).Should().NotThrow();
    }

    [Fact]
    public void Redirect_does_nothing()
    {
        response.Invoking(r => r.Redirect("", false)).Should().NotThrow();
    }
}
