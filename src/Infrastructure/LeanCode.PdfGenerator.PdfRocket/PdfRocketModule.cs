using Autofac;
using LeanCode.Configuration;
using Microsoft.Extensions.Configuration;

namespace LeanCode.PdfGenerator.PdfRocket
{
    class PdfRocketModule : Module
    {
        private readonly IConfiguration config;

        public PdfRocketModule(IConfiguration config)
        {
            this.config = config;
        }
        protected override void Load(ContainerBuilder builder)
        {
            if (config != null)
            {
                builder.ConfigSection<PdfRocketConfiguration>(config);
            }
            builder.RegisterType<PdfRocketGenerator>().As<IPdfGenerator>().SingleInstance();
        }
    }
}
