using Autofac.Core;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LeanCode.Components;

namespace LeanCode.EmailSender.SendGrid
{
    public class SendGridComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile { get; }

        public SendGridComponent(IConfiguration configuration)
        {
            AutofacModule = new SendGridModule(configuration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            
        }
    }
}
