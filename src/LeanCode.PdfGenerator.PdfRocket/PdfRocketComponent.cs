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

        [Obsolete("Use WithConfiguration/WithoutConfiguration factory methods.")]
        public PdfRocketComponent(IConfiguration config)
            : this(config, true)
        { }

        private PdfRocketComponent(IConfiguration config, bool useConfig)
        {
            if (useConfig && config == null)
            {
                throw new ArgumentNullException("Provide config when using configuration.", nameof(config));
            }
            else if (!useConfig && config != null)
            {
                throw new ArgumentNullException("Do not provide config, when config is not used.", nameof(config));
            }
            AutofacModule = new PdfRocketModule(config);
        }

        public void ConfigureServices(IServiceCollection services)
        { }

        public static PdfRocketComponent WithoutConfiguration() => new PdfRocketComponent(null, false);
        public static PdfRocketComponent WithConfiguration(IConfiguration config) => new PdfRocketComponent(config, true);
    }
}
