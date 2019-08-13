using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeanCode.ViewRenderer.Razor.ViewBase;
using Microsoft.AspNetCore.Razor.Language;
using Serilog;
using Xunit;

namespace LeanCode.ViewRenderer.Razor.Tests
{
    public class ViewCompilerTests
    {
        private readonly ViewLocator locator;
        private readonly ViewCompiler compiler;

        public ViewCompilerTests()
        {
            locator = new ViewLocator(new RazorViewRendererOptions("./Views/Compiler/"));
            compiler = new ViewCompiler(locator);
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

            var realType = result.ViewType;
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
        public async Task Layout_specification_on_non_first_line_works_correctly()
        {
            var result = await Compile("LayoutedSecondLine");

            Assert.Equal("Simple", result.Layout);
        }

        [Fact]
        public async Task Projected_size_is_equal_to_file_size()
        {
            var result = await Compile("Layouted");

            Assert.Equal(GetSize("Layouted"), result.ProjectedSize);
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

        [Fact]
        public async Task Generating_views_with_dots_in_file_name_works()
        {
            var result = await Compile("Dotted.Name");

            Assert.NotNull(result.ViewType);
        }

        private Task<CompiledView> Compile(string viewName)
        {
            return compiler.Compile(locator.GetItem(viewName));
        }

        private int GetSize(string name)
        {
            return (int)new FileInfo(locator.GetItem(name).PhysicalPath).Length;
        }
    }
}
