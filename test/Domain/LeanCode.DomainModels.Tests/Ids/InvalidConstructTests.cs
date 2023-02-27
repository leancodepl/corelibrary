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
}
