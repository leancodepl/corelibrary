using System;
using Autofac;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.SmsSender
{
    public class SmsSenderModule : AppModule
    {
        public override void ConfigureServices(IServiceCollection services) =>
            services.AddHttpClient<ISmsSender, SmsApiClient>(c => c.BaseAddress = new Uri(SmsApiClient.ApiBase));
    }
}
