using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace LeanCode.ViewRenderer.Razor
{
    class RazorViewRenderer : IViewRenderer
    {
        private readonly Serilog.ILogger logger
            = Serilog.Log.ForContext<RazorViewRenderer>();

        private readonly IRazorViewEngine viewEngine;
        private readonly ITempDataProvider tempDataProvider;
        private readonly IServiceProvider serviceProvider;

        public RazorViewRenderer(IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
        {
            this.viewEngine = viewEngine;
            this.tempDataProvider = tempDataProvider;
            this.serviceProvider = serviceProvider;
        }

        public string RenderToString<TModel>(string viewName, TModel model)
        {
            var actionContext = GetActionContext();

            logger.Verbose("Locating view for {ViewName}", viewName);
            var viewEngineResult = viewEngine.FindView(actionContext, viewName, false);
            if (!viewEngineResult.Success)
            {
                logger.Error("Cannot locate view for {ViewName}", viewName);
                throw new InvalidOperationException($"Couldn't find view '{viewName}'");
            }

            logger.Verbose("View for {ViewName} found at {Location}",
                viewName, viewEngineResult.View.Path);
            var view = viewEngineResult.View;

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary<TModel>(
                        metadataProvider: new EmptyModelMetadataProvider(),
                        modelState: new ModelStateDictionary())
                    {
                        Model = model
                    },
                    new TempDataDictionary(
                        actionContext.HttpContext,
                        tempDataProvider),
                    output,
                    new HtmlHelperOptions());

                view.RenderAsync(viewContext).Wait();

                logger.Debug("View {ViewName} rendered using {Location}",
                    viewName, viewEngineResult.View.Path);
                return output.ToString();
            }
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }
    }
}
