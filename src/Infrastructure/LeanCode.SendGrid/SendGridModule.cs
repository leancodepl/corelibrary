using Autofac;
using LeanCode.Components;
using SendGrid;

namespace LeanCode.SendGrid;

public class SendGridModule : AppModule
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(context => new SendGridClient(context.Resolve<SendGridClientOptions>()))
            .AsSelf();

        builder.RegisterType<SendGridRazorClient>()
            .AsSelf();
    }
}
