using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace LeanCode.ViewRenderer.Razor
{
    class ViewRendererRazorViewLocationExpander : IViewLocationExpander
    {
        private readonly ViewRendererOptions viewRendererOptions;

        public ViewRendererRazorViewLocationExpander(ViewRendererOptions viewRendererOptions)
        {
            this.viewRendererOptions = viewRendererOptions;
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            return viewLocations.Concat(viewRendererOptions.ViewLocations);
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        { }
    }
}
