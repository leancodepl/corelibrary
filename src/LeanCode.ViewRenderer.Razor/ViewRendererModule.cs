using Autofac;
using LeanCode.ViewRenderer.Razor;

namespace LeanCode.ViewRenderer.Razor
{
    class ViewRendererModule : Module
    {
        public ViewRendererModule()
        { }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RazorViewRenderer>().As<IViewRenderer>();
        }
    }
}
