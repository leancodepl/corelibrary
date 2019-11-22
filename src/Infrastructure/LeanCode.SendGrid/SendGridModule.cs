using Autofac;
using LeanCode.Components;

namespace LeanCode.SendGrid
{
    public class SendGridModule : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SendGridRazorClient>()
                .AsSelf();
        }
    }
}
