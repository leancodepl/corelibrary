using System;
using System.Net.Http.Headers;
using Autofac;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.EmailSender.SendGrid
{
    public class SendGridModule : AppModule
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient<SendGridHttpClient>()
                .ConfigureHttpClient((sp, c) =>
                {
                    var cfg = sp.GetService<SendGridConfiguration>();
                    c.BaseAddress = new Uri("https://api.sendgrid.com/v3/");
                    c.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", cfg.ApiKey);
                });
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SendGridClient>().As<IEmailClient>();
        }
    }
}
