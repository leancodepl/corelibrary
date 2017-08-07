using Autofac;
using Microsoft.Extensions.Configuration;
using LeanCode.SmsSender.SmsApi;
using LeanCode.Configuration;

namespace LeanCode.SmsSender
{
    class SmsSenderModule : Module
    {
        private readonly IConfiguration configuration;

        public SmsSenderModule(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.ConfigSection<SmsApiConfiguration>(configuration);

            builder.RegisterType<SmsApiSender>().As<ISmsSender>();
        }
    }
}
