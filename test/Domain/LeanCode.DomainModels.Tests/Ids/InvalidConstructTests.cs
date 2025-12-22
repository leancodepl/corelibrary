using Xunit;

namespace LeanCode.DomainModels.Tests.Ids;

public class InvalidConstructTests
{
    [Fact]
    public void No_partial()
    {
        const string Source =
            "using LeanCode.DomainModels.Ids; [TypedId(TypedIdFormat.RawGuid)] public readonly record struct Id;";

        var diag = GeneratorRunner.RunDiagnostics(Source);
        Assert.Single(diag, d => d.Id == "LNCD0005");
    }

    [Fact]
    public void No_partial_readonly()
    {
        const string Source =
            "using LeanCode.DomainModels.Ids; [TypedId(TypedIdFormat.RawGuid)] public record struct Id;";

        var diag = GeneratorRunner.RunDiagnostics(Source);
        Assert.Single(diag, d => d.Id == "LNCD0005");
    }

    [Fact]
    public void No_readonly()
    {
        const string Source =
            "using LeanCode.DomainModels.Ids; [TypedId(TypedIdFormat.RawGuid)] public partial record struct Id;";

        var diag = GeneratorRunner.RunDiagnostics(Source);
        Assert.Single(diag, d => d.Id == "LNCD0005");
    }

    [Fact]
    public void No_max_value_length_for_raw_string_id()
    {
        const string Source =
            "using LeanCode.DomainModels.Ids; [TypedId(TypedIdFormat.RawString)] public readonly partial record struct Id;";

        var diag = GeneratorRunner.RunDiagnostics(Source);
        Assert.Single(diag, d => d.Id == "LNCD0012");
    }

    [Fact]
    public void No_max_value_length_for_prefixed_string_id()
    {
        const string Source =
            "using LeanCode.DomainModels.Ids; [TypedId(TypedIdFormat.PrefixedString)] public readonly partial record struct Id;";

        var diag = GeneratorRunner.RunDiagnostics(Source);
        Assert.Single(diag, d => d.Id == "LNCD0012");
    }
}
