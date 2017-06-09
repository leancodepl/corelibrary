using System;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.PdfGenerator.PdfRocket
{
    public class PdfRocketComponent : IAppComponent
    {
        public IModule AutofacModule { get; }

        public Profile MapperProfile => null;

        public PdfRocketComponent(IConfiguration config)
        {
            AutofacModule = new PdfRocketModule(config);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
