using Autofac;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.SmsSender
{
    public class SmsSenderModule : AppModule
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SmsApiClient>().As<ISmsSender>();
        }
    }
}
