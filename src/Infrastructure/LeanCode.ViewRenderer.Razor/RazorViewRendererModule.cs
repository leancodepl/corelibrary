using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.ViewRenderer.Razor;

public class RazorViewRendererModule : AppModule
{
    private readonly RazorViewRendererOptions opts;

    public RazorViewRendererModule(RazorViewRendererOptions opts)
    {
        this.opts = opts;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddSingleton<IViewRenderer>(_ => new RazorViewRenderer(opts));
    }
}
