using System.Text;
using LeanCode.ViewRenderer.Razor.ViewBase;
using Microsoft.Extensions.Logging;

namespace LeanCode.ViewRenderer.Razor;

internal class RazorViewRenderer : IViewRenderer
{
    private readonly ILogger<RazorViewRenderer> logger;

    private readonly CompiledViewsCache cache;

    public RazorViewRenderer(
        ILogger<RazorViewRenderer> logger,
        ILogger<CompiledViewsCache> compiledViewsCacheLogger,
        ILogger<ViewLocator> viewLocatorLogger,
        ILogger<ViewCompiler> viewCompilerLogger,
        RazorViewRendererOptions options
    )
    {
        this.logger = logger;
        cache = new CompiledViewsCache(compiledViewsCacheLogger, viewLocatorLogger, viewCompilerLogger, options);
    }

    public async Task RenderToStreamAsync(
        string viewName,
        object model,
        Stream outputStream,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogDebug("Rendering view {ViewName}", viewName);

        await RenderAsync(outputStream, viewName, model, null);

        logger.LogInformation("View {ViewName} rendered", viewName);
    }

    public async Task<string> RenderToStringAsync(
        string viewName,
        object model,
        CancellationToken cancellationToken = default
    )
    {
        using (var ms = new MemoryStream())
        {
            await RenderToStreamAsync(viewName, model, ms, cancellationToken);

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }

    private async Task RenderAsync(Stream outputStream, string viewName, object model, BaseView? childView)
    {
        var compiledView = await cache.GetOrCompileAsync(viewName);

        var view = (BaseView?)Activator.CreateInstance(compiledView.ViewType);
        _ = view ?? throw new InvalidOperationException("Failed to create instance of compiled view type.");

        view.ChildView = childView;
        view.Model = model;

        if (string.IsNullOrEmpty(compiledView.Layout))
        {
            logger.LogDebug("Executing view object for view {ViewName}", viewName);

            await view.ExecuteAsync(outputStream);
        }
        else
        {
            logger.LogDebug("View {ViewName} has a layout {Layout}, delegating work", viewName, compiledView.Layout);

            await RenderAsync(outputStream, compiledView.Layout, model, view);
        }
    }
}
