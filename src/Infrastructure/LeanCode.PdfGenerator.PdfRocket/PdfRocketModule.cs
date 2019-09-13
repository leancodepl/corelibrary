using System;
using Autofac;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.PdfGenerator.PdfRocket
{
    public class PdfRocketModule : AppModule
    {
        public override void ConfigureServices(IServiceCollection services) =>
            services.AddHttpClient<PdfRocketHttpClient>(c => c.BaseAddress = new Uri("https://api.html2pdfrocket.com/"));

        protected override void Load(ContainerBuilder builder) =>
            builder.RegisterType<PdfRocketGenerator>().As<IPdfGenerator>();
    }
}
