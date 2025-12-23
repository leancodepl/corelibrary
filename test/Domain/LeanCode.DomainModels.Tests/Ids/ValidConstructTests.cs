using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace LeanCode.DomainModels.Tests.Ids;

public class ValidConstructTests
{
    [Fact]
    public void Correct_RawInt()
    {
        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.RawInt)]
                public readonly partial record struct Id;
            """
        );

        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.RawInt, CustomPrefix = "ignored")]
                public readonly partial record struct Id;
            """
        );

        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.RawInt, SkipRandomGenerator = true)]
                public readonly partial record struct Id;
            """
        );

        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.RawInt, CustomPrefix = "ignored", SkipRandomGenerator = true, MaxValueLength = 50)]
                public readonly partial record struct Id;
            """
        );
    }

    [Fact]
    public void Correct_RawLong()
    {
        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.RawLong)]
                public readonly partial record struct Id;
            """
        );

        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.RawLong, CustomPrefix = "ignored")]
                public readonly partial record struct Id;
            """
        );

        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.RawLong, SkipRandomGenerator = true)]
                public readonly partial record struct Id;
            """
        );

        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.RawLong, CustomPrefix = "ignored", SkipRandomGenerator = true, MaxValueLength = 50)]
                public readonly partial record struct Id;
            """
        );
    }

    [Fact]
    public void Correct_RawGuid()
    {
        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.RawGuid)]
                public readonly partial record struct Id;
            """
        );

        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.RawGuid, CustomPrefix = "ignored")]
                public readonly partial record struct Id;
            """
        );

        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.RawGuid, SkipRandomGenerator = true)]
                public readonly partial record struct Id;
            """
        );

        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.RawGuid, CustomPrefix = "ignored", SkipRandomGenerator = true, MaxValueLength = 50)]
                public readonly partial record struct Id;
            """
        );
    }

    [Fact]
    public void Correct_PrefixedGuid()
    {
        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.PrefixedGuid)]
                public readonly partial record struct Id;
            """
        );

        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.PrefixedGuid, CustomPrefix = "prefix")]
                public readonly partial record struct Id;
            """
        );

        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.PrefixedGuid, SkipRandomGenerator = true)]
                public readonly partial record struct Id;
            """
        );

        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.PrefixedGuid, CustomPrefix = "prefix", SkipRandomGenerator = true, MaxValueLength = 50)]
                public readonly partial record struct Id;
            """
        );
    }

    [Fact]
    public void Correct_RawString()
    {
        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.RawString, MaxValueLength = 100)]
                public readonly partial record struct Id;
            """
        );

        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.RawString, CustomPrefix = "ignored", SkipRandomGenerator = true, MaxValueLength = 50)]
                public readonly partial record struct Id;
            """
        );
    }

    [Fact]
    public void Correct_PrefixedString()
    {
        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.PrefixedString, MaxValueLength = 100)]
                public readonly partial record struct Id;
            """
        );

        AssertCorrect(
            """
                using LeanCode.DomainModels.Ids;
                namespace Test;
                [TypedId(TypedIdFormat.PrefixedString, CustomPrefix = "prefix", SkipRandomGenerator = true, MaxValueLength = 50)]
                public readonly partial record struct Id;
            """
        );
    }

    private static void AssertCorrect([StringSyntax("C#")] string source)
    {
        var diag = GeneratorRunner.RunDiagnostics(source);
        Assert.Empty(diag);
    }
}
