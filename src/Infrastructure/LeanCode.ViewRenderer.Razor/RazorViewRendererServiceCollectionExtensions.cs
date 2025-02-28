using LeanCode.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.ViewRenderer.Razor;

public static class RazorViewRendererServiceCollectionExtensions
{
    public static IServiceCollection AddRazorViewRenderer(
        this IServiceCollection services,
        RazorViewRendererOptions config
    )
    {
        services.TryAddSingleton<IViewRenderer>(sp => new RazorViewRenderer(
            sp.GetRequiredService<ILogger<RazorViewRenderer>>(),
            sp.GetRequiredService<ILogger<CompiledViewsCache>>(),
            sp.GetRequiredService<ILogger<ViewLocator>>(),
            sp.GetRequiredService<ILogger<ViewCompiler>>(),
            config
        ));
        return services;
    }
}
