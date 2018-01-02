using Autofac;
using LeanCode.Components;

namespace LeanCode.Mixpanel
{
    public class MixpanelModule : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MixpanelAnalytics>()
                .As<IMixpanelAnalytics>()
                .SingleInstance();
        }
    }

    public class MixpanelWithFactoryModule : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MixpanelAnalyticsFactory>()
                .As<IMixpanelAnalyticsFactory>()
                .SingleInstance();
        }
    }
}
