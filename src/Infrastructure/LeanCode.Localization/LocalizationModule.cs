using Autofac;
using Autofac.Core;
using LeanCode.Components;
using LeanCode.Localization.StringLocalizers;

namespace LeanCode.Localization;

public class LocalizationModule : AppModule
{
    private readonly LocalizationConfiguration config;

    public LocalizationModule(LocalizationConfiguration config)
    {
        this.config = config;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder
            .Register(c => new ResourceManagerStringLocalizer(config))
            .AsSelf()
            .AsImplementedInterfaces()
            .SingleInstance();
    }
}
