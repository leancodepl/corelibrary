using System;
using System.Net.Http.Headers;
using System.Text;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.SmsSender;

public class SmsSenderModule : AppModule
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddHttpClient<ISmsSender, SmsApiClient>((sp, c) =>
        {
            var config = sp.GetRequiredService<SmsApiConfiguration>();

            SmsApiClient.ConfigureHttpClient(config, c);
        });
}
