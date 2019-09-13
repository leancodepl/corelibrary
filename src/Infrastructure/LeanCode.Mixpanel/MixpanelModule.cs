using System;
using Autofac;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Mixpanel
{
    public class MixpanelModule : AppModule
    {
        public override void ConfigureServices(IServiceCollection services) =>
            services.AddHttpClient<MixpanelHttpClient>(c => c.BaseAddress = new Uri("https://api.mixpanel.com"));

        protected override void Load(ContainerBuilder builder) =>
            builder.RegisterType<MixpanelAnalytics>().AsSelf();
    }
}
