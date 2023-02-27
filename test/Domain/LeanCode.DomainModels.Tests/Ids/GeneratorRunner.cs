using System.Collections.Immutable;
using LeanCode.DomainModels.Generators;
using LeanCode.DomainModels.Ids;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LeanCode.DomainModels.Tests.Ids;

public static class GeneratorRunner
{
    private static readonly IReadOnlyList<PortableExecutableReference> DefaultAssemblies = new[]
    {
        LoadRefLib("System.Linq"),
        LoadRefLib("System.Linq.Expressions"),
        LoadRefLib("System.Memory"),
        LoadRefLib("System.Runtime"),
        LoadRefLib("System.Text.Json"),
        MetadataReference.CreateFromFile(typeof(TypedIdAttribute).Assembly.Location)
    };

    private static PortableExecutableReference LoadRefLib(string name)
    {
        using var stream = typeof(InvalidConstructTests).Assembly.GetManifestResourceStream(name);
        return MetadataReference.CreateFromStream(stream!);
    }

    public static ImmutableArray<Diagnostic> RunDiagnostics(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: DefaultAssemblies,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        var generator = new TypedIdGenerator();

        CSharpGeneratorDriver
            .Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var generationDiagnostics);

        var newCompilationDiagnostics = newCompilation.GetDiagnostics();
        return newCompilationDiagnostics.AddRange(generationDiagnostics);
    }
}
