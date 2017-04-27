using Autofac.Core;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LeanCode.Components;
using LeanCode.ViewRenderer.Razor;

namespace LeanCode.ViewRenderer
{
    public class ViewRendererComponent : IAppComponent
    {
        public IModule AutofacModule { get; } = new ViewRendererModule();
        public Profile MapperProfile { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Add(ServiceDescriptor.Transient<IConfigureOptions<RazorViewEngineOptions>,
                ConfigureRazorViewEngineOptions>());

            services.Configure<ViewRendererOptions>(o =>
            {
                o.AddViewLocation("/EmailTemplates/{0}.cshtml");
            });
        }
    }
}
