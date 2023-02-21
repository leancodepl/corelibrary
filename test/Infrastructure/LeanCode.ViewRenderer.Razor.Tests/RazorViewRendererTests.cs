using System.Threading.Tasks;
using Xunit;

namespace LeanCode.ViewRenderer.Razor.Tests;

public class RazorViewRendererTests
{
    private readonly RazorViewRendererOptions options = new RazorViewRendererOptions("./Views/Renderer");

    private readonly RazorViewRenderer renderer;

    public RazorViewRendererTests()
    {
        renderer = new RazorViewRenderer(options);
    }

    [Fact]
    public async Task Renders_simple_view_correctly()
    {
        var result = await Render("Simple");

        Assert.Equal("SIMPLE", result);
    }

    [Fact]
    public async Task Renders_simple_layout_correctly()
    {
        var result = await Render("Layouted");

        Assert.Equal("LAYOUT-PRE\nCONTENT\nLAYOUT-POST", result);
    }

    [Fact]
    public async Task Renders_hierarchical_layout_correctly()
    {
        var result = await Render("HierarchicalLayout");

        Assert.Equal("HL2-PRE\nHL1-PRE\nCONTENT\nHL1-POST\nHL2-POST", result);
    }

    [Fact]
    public async Task Throws_when_view_cannot_be_found()
    {
        await Assert.ThrowsAsync<ViewNotFoundException>(() => Render("NotExisting"));
    }

    [Fact]
    public async Task Correctly_renders_views_with_model()
    {
        var result = await Render("Model", new TestModel { Child = "CHILD_CONTENT" });

        Assert.Equal("CHILD_CONTENT", result);
    }

    [Fact]
    public async Task Passes_the_model_to_layouts()
    {
        var result = await Render("LayoutModel", new TestModel { Child = "CHILD", Layout = "MODEL" });

        Assert.Equal("PRE-MODEL\nCHILD\nPOST-MODEL", result);
    }

    [Fact]
    public async Task Correctly_renders_functions()
    {
        var result = await Render("Functions");

        Assert.Equal("Hello", result);
    }

    private Task<string> Render(string viewName, TestModel model = null) => renderer.RenderToStringAsync(viewName, model);
}

public class TestModel
{
    public string Child { get; set; }
    public string Layout { get; set; }
}
