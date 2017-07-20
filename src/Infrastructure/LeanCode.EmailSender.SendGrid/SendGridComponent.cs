using Autofac.Core;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LeanCode.Components;
using System;

namespace LeanCode.EmailSender.SendGrid
{
    public class SendGridComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile { get; }

        [Obsolete("Use WithConfiguration/WithoutConfiguration factory methods.")]
        public SendGridComponent(IConfiguration config)
            : this(config, true)
        { }

        private SendGridComponent(IConfiguration config, bool useConfig)
        {
            if (useConfig && config == null)
            {
                throw new ArgumentNullException("Provide config when using configuration.", nameof(config));
            }
            else if (!useConfig && config != null)
            {
                throw new ArgumentNullException("Do not provide config, when config is not used.", nameof(config));
            }
            AutofacModule = new SendGridModule(config);
        }

        public void ConfigureServices(IServiceCollection services)
        { }

        public static SendGridComponent WithoutConfiguration() => new SendGridComponent(null, false);
        public static SendGridComponent WithConfiguration(IConfiguration config) => new SendGridComponent(config, true);
    }
}
