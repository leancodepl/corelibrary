using System;
using Autofac;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Facebook
{
    public class FacebookModule : AppModule
    {
        public override void ConfigureServices(IServiceCollection services) =>
            services.AddHttpClient<FacebookClient>(c => c.BaseAddress = new Uri(FacebookClient.ApiBase));
    }
}
