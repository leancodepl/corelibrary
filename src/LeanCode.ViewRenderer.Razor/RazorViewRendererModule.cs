using Autofac;

namespace LeanCode.ViewRenderer.Razor
{
    class RazorViewRendererModule : Module
    {
        private readonly RazorViewRendererOptions opts;

        public RazorViewRendererModule(RazorViewRendererOptions opts)
        {
            this.opts = opts;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(_ => new RazorViewRenderer(opts))
                .As<IViewRenderer>()
                .SingleInstance();
        }
    }
}
