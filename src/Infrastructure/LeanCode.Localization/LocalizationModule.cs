using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Localization;

namespace LeanCode.Localization
{
    public class LocalizationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ResourceManagerStringLocalizerFactory>()
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();

            builder.Register(c => c.Resolve<IStringLocalizerFactory>()
                .Create(c.Resolve<LocalizationConfiguration>().ResourceSource))
                .AsImplementedInterfaces()
                .OnlyIf(reg => reg.IsRegistered(
                    new TypedService(typeof(LocalizationConfiguration))));
        }
    }
}
