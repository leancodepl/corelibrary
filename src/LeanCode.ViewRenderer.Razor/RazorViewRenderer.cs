using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LeanCode.ViewRenderer.Razor.ViewBase;

namespace LeanCode.ViewRenderer.Razor
{
    class RazorViewRenderer : IViewRenderer
    {
        private readonly CompiledViewsCache cache;

        public RazorViewRenderer(RazorViewRendererOptions options)
        {
            cache = new CompiledViewsCache(options);
        }

        public Task RenderToStream<TModel>(string viewName, TModel model, Stream outputStream)
        {
            return Render(outputStream, viewName, model, null, 0);
        }

        public async Task<string> RenderToString<TModel>(string viewName, TModel model)
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
                await view.ExecuteAsync(outputStream).ConfigureAwait(false);
            }
            else
            {
                await Render(outputStream, compiledView.Layout, model, view, childSize + compiledView.ProjectedSize).ConfigureAwait(false);
            }
        }
    }
}
