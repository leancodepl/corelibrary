using Autofac;
using Microsoft.Extensions.Configuration;
using LeanCode.Configuration;

namespace LeanCode.EmailSender.SendGrid
{
    class SendGridModule : Module
    {
        private readonly IConfiguration configuration;

        public SendGridModule(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (configuration != null)
            {
                builder.ConfigSection<SendGridConfiguration>(configuration);
            }

            builder.RegisterType<SendGridClient>().As<IEmailClient>();
            builder.RegisterType<EmailSender>().As<IEmailSender>();
        }
    }
}
