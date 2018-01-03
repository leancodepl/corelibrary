using Autofac;
using LeanCode.Components;

namespace LeanCode.EmailSender.SendGrid
{
    public class SendGridModule : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SendGridClient>().As<IEmailClient>().SingleInstance();
        }
    }
}
