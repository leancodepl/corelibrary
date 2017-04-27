using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;

namespace LeanCode.ViewRenderer.Razor
{
    class ConfigureRazorViewEngineOptions : IConfigureOptions<RazorViewEngineOptions>
    {
        private readonly ViewRendererOptions viewRendererOptions;

        public ConfigureRazorViewEngineOptions(IOptions<ViewRendererOptions> viewRendererOptions)
        {
            this.viewRendererOptions = viewRendererOptions.Value;
        }

        public void Configure(RazorViewEngineOptions options)
        {
            options.ViewLocationExpanders.Add(new ViewRendererRazorViewLocationExpander(viewRendererOptions));
        }
    }
}
