using Autofac;
using LeanCode.ViewRenderer.Razor;

namespace LeanCode.ViewRenderer.Razor
{
    class RazorViewRendererModule : Module
    {
        public RazorViewRendererModule(RazorViewRendererOptions opts)
        { }

        protected override void Load(ContainerBuilder builder)
        {
            // builder.RegisterType<RazorViewRenderer>().As<IViewRenderer>();
        }
    }
}
