using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeanCode.ViewRenderer.Razor.ViewBase;
using Xunit;

namespace LeanCode.ViewRenderer.Razor.Tests
{
    public class ViewCompilerTests
    {
        private readonly ViewCompiler compiler;


        public ViewCompilerTests()
        {
            compiler = new ViewCompiler();
        }

        [Fact]
        public async Task Correctly_compiles_view()
        {
            var result = await Compile("Simple");

            Assert.NotNull(result.ViewType);
        }

        [Fact]
        public async Task Throws_when_there_are_syntax_errors()
        {
            await Assert.ThrowsAsync<CompilationFailedException>(() => Compile("RazorError"));
        }

        [Fact]
        public async Task Throws_when_there_are_compiled_code_errors()
        {
            await Assert.ThrowsAsync<CompilationFailedException>(() => Compile("CompiledError"));
        }

        [Fact]
        public async Task Generated_type_derives_from_BaseView()
        {
            var result = await Compile("Simple");

            var realType = result.ViewType.GetTypeInfo();
            Assert.True(realType.BaseType == typeof(BaseView));
        }

        [Fact]
        public async Task Generated_type_can_be_constructed_without_params()
        {
            var result = await Compile("Simple");

            Assert.NotNull(Activator.CreateInstance(result.ViewType));
        }

        [Fact]
        public async Task Projected_size_is_equal_to_file_size_if_no_layout_is_specified()
        {
            var result = await Compile("Simple");

            Assert.Equal(GetSize("Simple"), result.ProjectedSize);
        }

        [Fact]
        public async Task Correctly_extracts_layout_from_first_line()
        {
            var result = await Compile("Layouted");

            Assert.Equal("Simple", result.Layout);
        }

        [Fact]
        public async Task Layout_specification_on_invalid_line_results_in_error()
        {
            await Assert.ThrowsAsync<CompilationFailedException>(() => Compile("LayoutedInvalidLine"));
        }

        [Fact]
        public async Task Projected_size_is_equal_to_file_size_minus_layout_directive_if_specified()
        {
            var result = await Compile("Layouted");

            Assert.Equal(GetSize("Layouted") - 17, result.ProjectedSize);
        }

        [Fact]
        public async Task Generated_type_renders_correct_view()
        {
            var result = await Compile("Simple");

            var view = (BaseView)Activator.CreateInstance(result.ViewType);
            string content;
            using (var ms = new MemoryStream())
            {
                await view.ExecuteAsync(ms);
                content = Encoding.UTF8.GetString(ms.ToArray());
            }

            Assert.Equal("this is simple view\n", content);
        }

        [Fact]
        public async Task Generated_type_with_layout_renders_correctly()
        {
            var result = await Compile("Layouted");

            var view = (BaseView)Activator.CreateInstance(result.ViewType);
            string content;
            using (var ms = new MemoryStream())
            {
                await view.ExecuteAsync(ms);
                content = Encoding.UTF8.GetString(ms.ToArray());
            }

            Assert.Equal("this is simple view\n", content);
        }

        private Task<CompiledView> Compile(string viewName) => compiler.Compile(View(viewName));
        private static string View(string name) => Path.GetFullPath($"./Views/Compiler/{name}.cshtml");

        private static int GetSize(string name)
        {
            using (var f = File.OpenRead(View(name)))
            {
                return (int)f.Length;
            }
        }
    }
}
