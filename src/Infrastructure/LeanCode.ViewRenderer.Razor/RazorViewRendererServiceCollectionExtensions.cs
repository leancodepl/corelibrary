using LeanCode.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.ViewRenderer.Razor;

public static class RazorViewRendererServiceCollectionExtensions
{
    public static IServiceCollection AddRazorViewRenderer(
        this IServiceCollection services,
        RazorViewRendererOptions config
    )
    {
        return services.AddSingleton<IViewRenderer>(sp => new RazorViewRenderer(
            config,
            sp.GetRequiredService<ILogger<RazorViewRenderer>>()
        ));
    }
}
