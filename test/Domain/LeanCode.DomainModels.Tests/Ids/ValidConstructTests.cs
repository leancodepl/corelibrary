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
                [TypedId(TypedIdFormat.RawInt, CustomPrefix = "ignored", SkipRandomGenerator = true)]
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
                [TypedId(TypedIdFormat.RawLong, CustomPrefix = "ignored", SkipRandomGenerator = true)]
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
                [TypedId(TypedIdFormat.RawGuid, CustomPrefix = "ignored", SkipRandomGenerator = true)]
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
                [TypedId(TypedIdFormat.PrefixedGuid, CustomPrefix = "prefix", SkipRandomGenerator = true)]
                public readonly partial record struct Id;
            """
        );
    }

    private static void AssertCorrect(string source)
    {
        var diag = GeneratorRunner.RunDiagnostics(source);
        Assert.Empty(diag);
    }
}
