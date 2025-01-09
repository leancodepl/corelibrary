using FluentAssertions;
using LeanCode.CQRS.AspNetCore.Registration;
using LeanCode.CQRS.Execution;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Registration;

public class ApiDescriptionTagsTests
{
    private static readonly CQRSObjectMetadata Metadata = new CQRSObjectMetadata(
        CQRSObjectKind.Command,
        typeof(ApiDescriptionTagsTests),
        typeof(ApiDescriptionTagsTests),
        typeof(ApiDescriptionTagsTests),
        (_, _) => Task.FromResult<object?>(null)
    );

    [Fact]
    public void FullNamespace_returns_full_namespace_with_dot_replaced()
    {
        var tags = ApiDescriptionTags.FullNamespace(null!, Metadata);

        tags.Should().BeEquivalentTo("LeanCode - CQRS - AspNetCore - Tests - Registration");
    }

    [Fact]
    public void SkipNamespacePrefix_skips_initial_namespace_parts()
    {
        Tag(skip: 0).Should().BeEquivalentTo("LeanCode - CQRS - AspNetCore - Tests - Registration");
        Tag(skip: 1).Should().BeEquivalentTo("CQRS - AspNetCore - Tests - Registration");
        Tag(skip: 2).Should().BeEquivalentTo("AspNetCore - Tests - Registration");
        Tag(skip: 10).Should().BeEquivalentTo("");

        IReadOnlyList<string> Tag(int skip) => ApiDescriptionTags.SkipNamespacePrefix(skip)(null!, Metadata);
    }
}
