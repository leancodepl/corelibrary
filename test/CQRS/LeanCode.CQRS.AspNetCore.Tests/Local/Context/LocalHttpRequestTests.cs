using FluentAssertions;
using LeanCode.CQRS.AspNetCore.Local.Context;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Local.Context;

public class LocalHttpRequestTests
{
    private readonly LocalHttpRequest request = new(Substitute.For<HttpContext>(), null);

    [Fact]
    public void Headers_are_empty()
    {
        request.Headers.Should().BeEmpty();
    }

    [Fact]
    public void HasFormContentType_is_always_false()
    {
        request.HasFormContentType.Should().BeFalse();
    }

    [Fact]
    public void Method_is_empty_and_cannot_be_changed()
    {
        request.Method.Should().BeEmpty();
        request.Method = "GET";
        request.Method.Should().BeEmpty();
    }

    [Fact]
    public void Scheme_is_empty_and_cannot_be_changed()
    {
        request.Scheme.Should().BeEmpty();
        request.Scheme = "http";
        request.Scheme.Should().BeEmpty();
    }

    [Fact]
    public void IsHttps_is_false_and_cannot_be_changed()
    {
        request.IsHttps.Should().BeFalse();
        request.IsHttps = true;
        request.IsHttps.Should().BeFalse();
    }

    [Fact]
    public void Host_is_empty_and_cannot_be_changed()
    {
        request.Host.Should().Be(default(HostString));
        request.Host = new HostString("localhost");
        request.Host.Should().Be(default(HostString));
    }

    [Fact]
    public void PathBase_is_empty_and_cannot_be_changed()
    {
        request.PathBase.Should().Be(PathString.Empty);
        request.PathBase = new("/other");
        request.PathBase.Should().Be(PathString.Empty);
    }

    [Fact]
    public void Path_is_empty_and_cannot_be_changed()
    {
        request.Path.Should().Be(PathString.Empty);
        request.Path = new("/other");
        request.Path.Should().Be(PathString.Empty);
    }

    [Fact]
    public void QueryString_is_empty_and_cannot_be_changed()
    {
        request.QueryString.Should().Be(QueryString.Empty);
        request.QueryString = new("?other");
        request.QueryString.Should().Be(QueryString.Empty);
    }

    [Fact]
    public void Query_is_empty_query_collection_and_cannot_be_changed()
    {
        request.Query.Should().BeSameAs(QueryCollection.Empty);
        request.Query = new QueryCollection();
        request.Query.Should().BeSameAs(QueryCollection.Empty);
    }

    [Fact]
    public void Protocol_is_empty_and_cannot_be_changed()
    {
        request.Protocol.Should().BeEmpty();
        request.Protocol = "HTTP/1.1";
        request.Protocol.Should().BeEmpty();
    }

    [Fact]
    public void ContentLength_is_null_and_cannot_be_changed()
    {
        request.ContentLength.Should().BeNull();
        request.ContentLength = 10;
        request.ContentLength.Should().BeNull();
    }

    [Fact]
    public void ContentType_is_null_and_cannot_be_changed()
    {
        request.ContentType.Should().BeNull();
        request.ContentType = "application/json";
        request.ContentType.Should().BeNull();
    }

    [Fact]
    public void Body_is_null_stream_and_cannot_be_changed()
    {
        request.Body.Should().BeSameAs(Stream.Null);
        request.Body = Substitute.For<Stream>();
        request.Body.Should().BeSameAs(Stream.Null);
    }

    [Fact]
    public void Form_is_empty_form_collection_and_cannot_be_changed()
    {
        request.Form.Should().BeSameAs(FormCollection.Empty);
        request.Form = new FormCollection(null);
        request.Form.Should().BeSameAs(FormCollection.Empty);
    }

    [Fact]
    public void Headers_dictionary_is_passed_further()
    {
        var headers = new HeaderDictionary { ["X-TEST"] = "test" };
        var request = new LocalHttpRequest(Substitute.For<HttpContext>(), headers);

        request.Headers.Should().BeSameAs(headers);
    }
}
