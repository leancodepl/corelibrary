using FluentAssertions;
using LeanCode.CQRS.AspNetCore.Local.Context;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Local.Context;

public class NullHeaderDictionaryTests
{
    private readonly NullHeaderDictionary headers = NullHeaderDictionary.Empty;

    [Fact]
    public void Indexer_always_returns_empty()
    {
        headers["key"].Should().BeEmpty();
        headers["key"] = "value";
        headers["key"].Should().BeEmpty();
    }

    [Fact]
    public void ContentLength_always_returns_null()
    {
        headers.ContentLength.Should().BeNull();

        headers.ContentLength = 123;

        headers.ContentLength.Should().BeNull();
    }

    [Fact]
    public void Keys_is_empty_and_readonly()
    {
        headers.Keys.Should().BeEmpty();
        headers.Keys.IsReadOnly.Should().BeTrue();
    }

    [Fact]
    public void Values_is_empty()
    {
        headers.Values.Should().BeEmpty();
        headers.Values.IsReadOnly.Should().BeTrue();
    }

    [Fact]
    public void Count_is_zero()
    {
        headers.Count.Should().Be(0);
    }

    [Fact]
    public void IsReadOnly_is_true()
    {
        headers.IsReadOnly.Should().BeTrue();
    }

    [Fact]
    public void Add_does_nothing()
    {
        headers.Add("key", "value");
        headers.Keys.Should().BeEmpty();
        headers.Values.Should().BeEmpty();
        headers.Count.Should().Be(0);
    }

    [Fact]
    public void Add_with_key_value_pair_does_nothing()
    {
        headers.Add(new KeyValuePair<string, StringValues>("key", StringValues.Empty));
        headers.Keys.Should().BeEmpty();
        headers.Values.Should().BeEmpty();
        headers.Count.Should().Be(0);
    }

    [Fact]
    public void Clear_does_nothing()
    {
        headers.Clear();
    }

    [Fact]
    public void Contains_always_returns_false()
    {
        headers.Contains(new KeyValuePair<string, StringValues>("key", StringValues.Empty)).Should().BeFalse();
    }

    [Fact]
    public void ContainsKey_always_returns_false()
    {
        headers.ContainsKey("key").Should().BeFalse();
    }

    [Fact]
    public void CopyTo_does_nothing()
    {
        var output = Array.Empty<KeyValuePair<string, StringValues>>();
        headers.CopyTo(output, 0);
        output.Should().BeEmpty();
    }

    [Fact]
    public void Remove_always_returns_false()
    {
        headers.Remove("key").Should().BeFalse();
    }

    [Fact]
    public void Remove_with_key_value_pair_always_returns_false()
    {
        headers.Remove(new KeyValuePair<string, StringValues>("key", StringValues.Empty)).Should().BeFalse();
    }

    [Fact]
    public void TryGetValue_always_returns_false_and_empty()
    {
        headers.TryGetValue("key", out var value).Should().BeFalse();
        value.Should().BeEmpty();
    }
}
