using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LeanCode.ViewRenderer.Razor.ViewBase;

namespace LeanCode.ViewRenderer.Razor
{
    internal class RazorViewRenderer : IViewRenderer
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<RazorViewRenderer>();

        private readonly CompiledViewsCache cache;

        public RazorViewRenderer(RazorViewRendererOptions options)
        {
            cache = new CompiledViewsCache(options);
        }

        public async Task RenderToStream(string viewName, object model, Stream outputStream)
        {
            logger.Debug("Rendering view {ViewName}", viewName);
            await Render(outputStream, viewName, model, null, 0).ConfigureAwait(false);
            logger.Information("View {ViewName} rendered", viewName);
        }

        public async Task<string> RenderToString(string viewName, object model)
        {
            using (var ms = new MemoryStream())
            {
                await RenderToStream(viewName, model, ms);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private async Task Render(Stream outputStream, string viewName, object model, BaseView childView, int childSize)
        {
            var compiledView = await cache.GetOrCompile(viewName);
            var view = (BaseView)Activator.CreateInstance(compiledView.ViewType);
            view.ChildView = childView;
            view.Model = model;

            if (string.IsNullOrEmpty(compiledView.Layout))
            {
                logger.Debug("Executing view object for view {ViewName}", viewName);
                await view.ExecuteAsync(outputStream).ConfigureAwait(false);
            }
            else
            {
                logger.Debug("View {ViewName} has a layout {Layout}, delegating work", viewName, compiledView.Layout);
                await Render(outputStream, compiledView.Layout, model, view, childSize + compiledView.ProjectedSize).ConfigureAwait(false);
            }
        }
    }
}
