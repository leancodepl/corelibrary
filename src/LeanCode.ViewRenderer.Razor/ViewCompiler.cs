using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using LeanCode.ViewRenderer.Razor.ViewBase;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace LeanCode.ViewRenderer.Razor
{
    struct CompiledView
    {
        public string Layout { get; }
        public Type ViewType { get; }
        public int ProjectedSize { get; }

        public CompiledView(string layout, Type viewType, int projectedSize)
        {
            Layout = layout;
            ViewType = viewType;
            ProjectedSize = projectedSize;
        }
    }

    class ViewCompiler
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<ViewCompiler>();

        private static readonly List<PortableExecutableReference> References = new[]
        {
            Assembly.Load(new AssemblyName("mscorlib")),
            Assembly.Load(new AssemblyName("System.Private.CoreLib")),
            Assembly.Load(new AssemblyName("System.Runtime")),
            Assembly.Load(new AssemblyName("System.Threading.Tasks")),
            Assembly.Load(new AssemblyName("System.ValueTuple")),
            Assembly.Load(new AssemblyName("System.IO")),
            Assembly.Load(new AssemblyName("Microsoft.CSharp")),
            Assembly.Load(new AssemblyName("System.Dynamic.Runtime")),
            typeof(ViewCompiler).GetTypeInfo().Assembly,
            typeof(HtmlEncoder).GetTypeInfo().Assembly,
        }
        .Distinct()
        .Select(a => MetadataReference.CreateFromFile(a.Location))
        .ToList();

        private static CSharpCompilationOptions Options = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary);

        private readonly RazorTemplateEngine engine;

        public ViewCompiler()
        {
            engine = PrepareEngine();
        }

        public async Task<CompiledView> Compile(string fullPath)
        {
            logger.Debug("Compiling view {ViewPath}", fullPath);
            var (layout, reader, size) = await ExtractLayout(fullPath);
            var code = await GenerateCode(fullPath, reader);
            logger.Debug("Code for view {ViewPath} generated", fullPath);
            var assembly = await Task.Run(() => GenerateAssembly(fullPath, code));
            var type = assembly.GetExportedTypes()[0];
            logger.Information("View {ViewPath} compiled to assembly {Assembly} to type {Type}", fullPath, assembly, type);
            return new CompiledView(layout, type, size);
        }

        private Assembly GenerateAssembly(string fullPath, string code)
        {
            SyntaxTree tree;
            try
            {
                var sourceCode = SourceText.From(code);
                tree = CSharpSyntaxTree.ParseText(sourceCode);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Cannot parse syntax tree for view {ViewPath}", fullPath);
                throw new CompilationFailedException(fullPath, new string[0], "Cannot parse syntax tree.", ex);
            }
            var assemblyName = typeof(ViewCompiler).FullName + ".GeneratedViews-" + Guid.NewGuid();
            var compilation = CSharpCompilation.Create(assemblyName, new[] { tree }, References, Options);

            using (var assemblyStream = new MemoryStream())
            {
                var compilationResult = compilation.Emit(assemblyStream);

                if (!compilationResult.Success)
                {
                    var errors = compilationResult.Diagnostics.Select(d => d.GetMessage()).ToList();
                    logger.Warning("Cannot emit IL to in-memory stream for view {ViewPath}, errors:", fullPath);
                    foreach (var err in errors)
                    {
                        logger.Warning("\t {Error}", err);
                    }

                    throw new CompilationFailedException(
                        fullPath,
                        errors,
                        "Cannot compile the generated code."
                        );
                }

                assemblyStream.Seek(0, SeekOrigin.Begin);

                try
                {
                    return AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
                }
                catch (Exception ex)
                {
                    logger.Warning(ex, "Cannot load compiled assembly for view {ViewPath}", fullPath);
                    throw new CompilationFailedException(
                        fullPath,
                        new string[0],
                        "Cannot load generated assembluy.",
                        ex
                    );
                }
            }
        }

        private async Task<string> GenerateCode(string fullPath, StreamReader reader)
        {
            GeneratorResults genResult;
            using (reader)
            {
                genResult = await Task.Run(() => engine.GenerateCode(reader));
            }

            if (!genResult.Success)
            {
                var errors = genResult.ParserErrors.Select(e => e.ToString()).ToList();
                logger.Warning("Cannot generate code for the view {ViewPath}, errors:", fullPath);
                foreach (var err in errors)
                {
                    logger.Warning("\t {Error}", err);
                }

                throw new CompilationFailedException(
                    fullPath,
                    errors,
                    "Cannot compile view - Razor syntax errors"
                );
            }

            return genResult.GeneratedCode;
        }

        private async Task<(string, StreamReader, int)> ExtractLayout(string fullPath)
        {
            StreamReader file;
            try
            {
                file = File.OpenText(fullPath);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Cannot open view {ViewPath}", fullPath);
                throw new CompilationFailedException(fullPath, new string[0], "Cannot open view file.", ex);
            }

            var size = (int)((FileStream)file.BaseStream).Length;
            var firstLine = await file.ReadLineAsync().ConfigureAwait(false);
            if (firstLine != null && firstLine.StartsWith("@layout "))
            {
                var quoted = firstLine.Substring("@layout ".Length);
                if (quoted[0] != '"' || quoted[quoted.Length - 1] != '"')
                {
                    logger.Warning("Cannot extract layout from the view {ViewPath} - invalid quotes", fullPath);
                    throw new CompilationFailedException(
                        fullPath,
                        new string[0],
                        "Cannot compile the view - invalid layout specification.");
                }
                return (quoted.Substring(1, quoted.Length - 2), file, size - firstLine.Length - 1);
            }
            else
            {
                file.BaseStream.Seek(0, SeekOrigin.Begin);
                file.DiscardBufferedData();
                return (null, file, size);
            }
        }

        private static RazorTemplateEngine PrepareEngine()
        {
            var codeLang = new CSharpRazorCodeLanguage();
            var host = new RazorEngineHost(codeLang);

            host.DefaultBaseClass = typeof(BaseView).FullName;
            host.GeneratedClassContext = new GeneratedClassContext(
                executeMethodName: nameof(BaseView.ExecuteAsync),
                writeMethodName: "Write", // accessibility
                writeLiteralMethodName: "WriteLiteral",
                writeToMethodName: "WriteTo",
                writeLiteralToMethodName: "WriteLiteralTo",
                templateTypeName: nameof(HelperResult),
                generatedTagHelperContext: new GeneratedTagHelperContext());
            host.NamespaceImports.Add("System");

            return new RazorTemplateEngine(host);
        }

    }
}
