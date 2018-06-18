using System;
using Autofac;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Facebook
{
    public class FacebookModule : AppModule
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FacebookClient>().AsSelf();
        }
    }
}
