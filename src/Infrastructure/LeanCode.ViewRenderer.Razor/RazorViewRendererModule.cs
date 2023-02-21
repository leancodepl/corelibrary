using Autofac;
using LeanCode.Components;

namespace LeanCode.ViewRenderer.Razor;

public class RazorViewRendererModule : AppModule
{
    private readonly RazorViewRendererOptions opts;

    public RazorViewRendererModule(RazorViewRendererOptions opts)
    {
        this.opts = opts;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(_ => new RazorViewRenderer(opts))
            .As<IViewRenderer>()
            .SingleInstance();
    }
}
