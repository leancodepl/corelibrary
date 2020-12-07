using Autofac;
using LeanCode.Components;

namespace LeanCode.OrderedHostedServices
{
    public class OrderedHostedServiceModule : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OrderedHostedServiceExecutor>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
