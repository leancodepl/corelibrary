using FluentAssertions;
using LeanCode.CQRS.AspNetCore.Local.Context;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Local.Context;

public class NullSessionTests
{
    [Fact]
    public void IsAvailable_should_always_be_false()
    {
        NullSession.Empty.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void Id_should_always_be_empty()
    {
        NullSession.Empty.Id.Should().BeEmpty();
    }

    [Fact]
    public void Keys_should_be_empty()
    {
        NullSession.Empty.Keys.Should().BeEmpty();
    }

    [Fact]
    public void Clear_should_not_throw()
    {
        NullSession.Empty.Clear();
    }

    [Fact]
    public async Task Commit_should_not_throw()
    {
        await NullSession.Empty.CommitAsync();
    }

    [Fact]
    public async Task Load_should_not_throw()
    {
        await NullSession.Empty.LoadAsync();
    }

    [Fact]
    public void Remove_should_not_throw()
    {
        NullSession.Empty.Remove("");
    }

    [Fact]
    public void Set_should_not_throw()
    {
        NullSession.Empty.Set("", Array.Empty<byte>());
    }

    [Fact]
    public void TryGetValue_should_always_return_false_and_null()
    {
        NullSession.Empty.TryGetValue("", out var value).Should().BeFalse();
        value.Should().BeNull();
    }
}
