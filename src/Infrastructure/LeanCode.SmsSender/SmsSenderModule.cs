using System;
using System.Net.Http.Headers;
using System.Text;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.SmsSender
{
    public class SmsSenderModule : AppModule
    {
        public override void ConfigureServices(IServiceCollection services) =>
            services.AddHttpClient<ISmsSender, SmsApiClient>((sp, c) =>
            {
                c.BaseAddress = new Uri(SmsApiClient.ApiBase);

                var config = sp.GetRequiredService<SmsApiConfiguration>();

                if (config.Token is { Length: > 0 } token)
                {
                    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.Login}:{config.Password}")));
                }
            });
    }
}
