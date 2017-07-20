using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.ViewRenderer.Razor
{
    public class RazorViewRendererComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile { get; }

        public RazorViewRendererComponent(RazorViewRendererOptions opts)
        {
            AutofacModule = new RazorViewRendererModule(opts);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
