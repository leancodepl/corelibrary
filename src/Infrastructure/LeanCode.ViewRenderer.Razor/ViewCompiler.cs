using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using LeanCode.ViewRenderer.Razor.ViewBase;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace LeanCode.ViewRenderer.Razor;

internal struct CompiledView
{
    public string? Layout { get; }
    public Type ViewType { get; }
    public int ProjectedSize { get; }

    public CompiledView(string? layout, Type viewType, int projectedSize)
    {
        Layout = layout;
        ViewType = viewType;
        ProjectedSize = projectedSize;
    }
}

internal class ViewCompiler
{
    private const string FilePreamble = @"@using System";

    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<ViewCompiler>();

    private static readonly List<PortableExecutableReference> References = new[]
    {
        Assembly.Load(new AssemblyName("mscorlib")),
        Assembly.Load(new AssemblyName("netstandard")),
        Assembly.Load(new AssemblyName("System.Runtime")),
        Assembly.Load(new AssemblyName("System.Private.CoreLib")),
        Assembly.Load(new AssemblyName("System.Threading.Tasks")),
        Assembly.Load(new AssemblyName("System.Linq.Expressions")),
        Assembly.Load(new AssemblyName("Microsoft.CSharp")),
        Assembly.Load(new AssemblyName("System.Dynamic.Runtime")),
        typeof(ViewCompiler).Assembly,
        typeof(HtmlEncoder).Assembly,
        typeof(RazorCompiledItem).Assembly,
    }
        .Distinct()
        .Select(a => MetadataReference.CreateFromFile(a.Location))
        .ToList();

    private static readonly CSharpCompilationOptions Options = new CSharpCompilationOptions(
        outputKind: OutputKind.DynamicallyLinkedLibrary
    );

    private readonly RazorProjectEngine engine;

    public ViewCompiler(ViewLocator locator)
    {
        engine = PrepareEngine(locator);
    }

    public async Task<CompiledView> CompileAsync(RazorProjectItem item)
    {
        logger.Debug("Compiling view {ViewPath}", item.PhysicalPath);

        var code = await GenerateCodeAsync(item);

        logger.Debug("Code for view {ViewPath} generated", item.PhysicalPath);

        var assembly = await Task.Run(() => GenerateAssembly(item.PhysicalPath, code));

        var type = assembly.GetExportedTypes()[0];

        logger.Information(
            "View {ViewPath} compiled to assembly {Assembly} to type {Type}",
            item.PhysicalPath,
            assembly,
            type
        );

        var field = type.GetField(Extensions.LayoutNode.LayoutFieldName);

        var layout = (string?)field?.GetValue(null);

        var size = new FileInfo(item.PhysicalPath).Length;

        return new CompiledView(layout, type, (int)size);
    }

    private Assembly GenerateAssembly(string fullPath, string code)
    {
        SyntaxTree tree;
        try
        {
            tree = CSharpSyntaxTree.ParseText(
                SourceText.From(code),
                CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp8)
            );
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Cannot parse syntax tree for view {ViewPath}", fullPath);

            throw new CompilationFailedException(fullPath, "Cannot parse syntax tree.", ex);
        }

        var assemblyName = typeof(ViewCompiler).FullName + ".GeneratedViews-" + Guid.NewGuid();

        var compilation = CSharpCompilation.Create(assemblyName, new[] { tree }, References, Options);

        using (var assemblyStream = new MemoryStream())
        {
            var compilationResult = compilation.Emit(assemblyStream);

            if (!compilationResult.Success)
            {
                var errors = compilationResult
                    .Diagnostics.Select(d => d.GetMessage(CultureInfo.InvariantCulture))
                    .ToList();

                logger.Warning("Cannot emit IL to in-memory stream for view {ViewPath}, errors:", fullPath);

                foreach (var err in errors)
                {
                    logger.Warning("\t {Error}", err);
                }

                throw new CompilationFailedException(fullPath, errors, "Cannot compile the generated code.");
            }

            assemblyStream.Seek(0, SeekOrigin.Begin);

            try
            {
                return AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Cannot load compiled assembly for view {ViewPath}", fullPath);

                throw new CompilationFailedException(fullPath, "Cannot load generated assembly.", ex);
            }
        }
    }

    private async Task<string> GenerateCodeAsync(RazorProjectItem item)
    {
        var genResult = await Task.Run(() => engine.Process(item).GetCSharpDocument());

        if (genResult.Diagnostics.Any(d => d.Severity == RazorDiagnosticSeverity.Error))
        {
            var errors = genResult.Diagnostics.Select(d => d.ToString()).ToList();

            logger.Warning("Cannot generate code for the view {ViewPath}, errors:", item.PhysicalPath);

            foreach (var err in errors)
            {
                logger.Warning("\t {Error}", err);
            }

            throw new CompilationFailedException(
                item.PhysicalPath,
                errors,
                "Cannot compile view - Razor syntax errors"
            );
        }
        else if (genResult.Diagnostics.Count > 0)
        {
            var diags = genResult.Diagnostics.Select(d => d.ToString()).ToList();

            logger.Information("Diagnostics for {ViewPath} compilation:", item.PhysicalPath);

            foreach (var diag in diags)
            {
                logger.Warning("\t {Diagnostic}", diag);
            }
        }

        return genResult.GeneratedCode;
    }

    private static RazorProjectEngine PrepareEngine(ViewLocator locator)
    {
        return RazorProjectEngine.Create(
            RazorConfiguration.Default,
            RazorProjectFileSystem.Create(locator.GetRootPath()),
            builder =>
            {
                builder.SetBaseType(typeof(BaseView).FullName);
                builder.ConfigureClass(
                    (doc, @class) =>
                    {
                        @class.ClassName = "View_" + Guid.NewGuid().ToString("N");
                        @class.Modifiers.Clear();
                        @class.Modifiers.Add("public");
                        @class.Modifiers.Add("sealed");
                    }
                );
                builder.AddDirective(Extensions.Layout.Directive);
                builder.Features.Add(new Extensions.LayoutDirectivePass());

                builder.AddDefaultImports(FilePreamble);
            }
        );
    }
}
