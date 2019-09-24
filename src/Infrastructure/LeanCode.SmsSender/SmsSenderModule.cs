using System;
using Autofac;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.SmsSender
{
    public class SmsSenderModule : AppModule
    {
        public override void ConfigureServices(IServiceCollection services) =>
            services.AddHttpClient<SmsApiHttpClient>(c => c.BaseAddress = new Uri(SmsApiClient.ApiBase));

        protected override void Load(ContainerBuilder builder) =>
            builder.RegisterType<SmsApiClient>().As<ISmsSender>();
    }
}
