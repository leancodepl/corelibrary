using Autofac;
using Microsoft.Extensions.Configuration;
using LeanCode.Configuration;

namespace LeanCode.Mixpanel
{
    class MixpanelModule : Module
    {
        private readonly IConfiguration configuration;

        public MixpanelModule(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.ConfigSection<MixpanelConfiguration>(configuration);

            builder.RegisterType<MixpanelAnalytics>().As<IMixpanelAnalytics>();
        }
    }
}
