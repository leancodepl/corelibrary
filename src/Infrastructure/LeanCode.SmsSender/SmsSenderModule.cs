using Autofac;
using LeanCode.Components;

namespace LeanCode.SmsSender
{
    public class SmsSenderModule : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SmsApiClient>().As<ISmsSender>().SingleInstance();
        }
    }
}
