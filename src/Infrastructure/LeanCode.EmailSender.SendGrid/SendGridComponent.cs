using Autofac.Core;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LeanCode.Components;
using System;

namespace LeanCode.EmailSender.SendGrid
{
    /// <summary>
    /// Registers SendGrid implementation of
    /// <see cref="LeanCode.EmailSender.IEmailClient" /> along with its
    /// configuration. If you don't want it, you can use
    /// <see cref="SendGridClient" /> directly.
    /// </summary>
    public class SendGridComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile { get; }

        private SendGridComponent(IConfiguration config)
        {
            AutofacModule = new SendGridModule(config);
        }

        public void ConfigureServices(IServiceCollection services)
        { }

        public static SendGridComponent WithoutConfiguration()
            => new SendGridComponent(null);
        public static SendGridComponent WithConfiguration(IConfiguration config)
            => new SendGridComponent(config);
    }
}
