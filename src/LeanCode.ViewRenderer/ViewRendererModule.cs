using Autofac;
using LeanCode.ViewRenderer.Razor;
using LeanCode.ViewRenderer.Templates;

namespace LeanCode.ViewRenderer
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
