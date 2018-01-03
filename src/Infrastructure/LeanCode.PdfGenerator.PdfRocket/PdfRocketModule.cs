using Autofac;
using LeanCode.Components;

namespace LeanCode.PdfGenerator.PdfRocket
{
    public class PdfRocketModule : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PdfRocketGenerator>().As<IPdfGenerator>().SingleInstance();
        }
    }
}
