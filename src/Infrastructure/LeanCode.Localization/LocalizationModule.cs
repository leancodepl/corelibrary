using Autofac;
using Autofac.Core;
using LeanCode.Components;
using LeanCode.Localization.StringLocalizers;

namespace LeanCode.Localization
{
    public class LocalizationModule : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ResourceManagerStringLocalizer>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
