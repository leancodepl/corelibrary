using System.Collections.Immutable;
using LeanCode.DomainModels.Generators;
using LeanCode.DomainModels.Ids;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace LeanCode.DomainModels.Tests.Ids;

public class InvalidConstructTests
{
    [Fact]
    public void Correct_declaration()
    {
        const string Source =
            "using LeanCode.DomainModels.Ids; [TypedId(TypedIdFormat.RawGuid)] public readonly partial record struct Id;";
        var diag = RunDiagnostics(Source);
        Assert.Empty(diag);
    }

    [Fact]
    public void No_partial()
    {
        const string Source =
            "using LeanCode.DomainModels.Ids; [TypedId(TypedIdFormat.RawGuid)] public readonly record struct Id;";

        var diag = RunDiagnostics(Source);
        Assert.Single(diag, d => d.Id == "LNCD0005");
    }

    [Fact]
    public void No_readonly()
    {
        const string Source =
            "using LeanCode.DomainModels.Ids; [TypedId(TypedIdFormat.RawGuid)] public partial record struct Id;";

        var diag = RunDiagnostics(Source);
        Assert.Single(diag, d => d.Id == "LNCD0005");
    }

    [Fact]
    public void No_partial_readonly()
    {
        const string Source =
            "using LeanCode.DomainModels.Ids; [TypedId(TypedIdFormat.RawGuid)] public record struct Id;";

        var diag = RunDiagnostics(Source);
        Assert.Single(diag, d => d.Id == "LNCD0005");
    }

    private static ImmutableArray<Diagnostic> RunDiagnostics(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: DefaultAssemblies,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        var generator = new TypedIdGenerator();

        return CSharpGeneratorDriver.Create(generator).RunGenerators(compilation).GetRunResult().Diagnostics;
    }

    private static readonly IReadOnlyList<PortableExecutableReference> DefaultAssemblies = new[]
    {
        LoadSystemRuntime(),
        MetadataReference.CreateFromFile(typeof(TypedIdAttribute).Assembly.Location)
    };

    private static PortableExecutableReference LoadSystemRuntime()
    {
        using var stream = typeof(InvalidConstructTests).Assembly.GetManifestResourceStream("net70.System.Runtime");
        return MetadataReference.CreateFromStream(stream!);
    }
}
