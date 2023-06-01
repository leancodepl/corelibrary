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
        services.TryAddSingleton<IViewRenderer>(_ => new RazorViewRenderer(config));
        return services;
    }
}
