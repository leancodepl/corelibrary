using FluentAssertions;
using LeanCode.CQRS.AspNetCore.Local.Context;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Local.Context;

public class NullRequestCookieCollectionTests
{
    [Fact]
    public void Count_should_be_zero()
    {
        NullRequestCookieCollection.Empty.Count.Should().Be(0);
    }

    [Fact]
    public void ContainsKey_should_always_return_false()
    {
        NullRequestCookieCollection.Empty.ContainsKey("").Should().BeFalse();
    }

    [Fact]
    public void GetEnumerator_should_return_empty_enumerable()
    {
        NullRequestCookieCollection.Empty.GetEnumerator().MoveNext().Should().BeFalse();
    }

    [Fact]
    public void TryGetValue_should_always_return_false_and_null()
    {
        NullRequestCookieCollection.Empty.TryGetValue("", out var value).Should().BeFalse();
        value.Should().BeNull();
    }
}
