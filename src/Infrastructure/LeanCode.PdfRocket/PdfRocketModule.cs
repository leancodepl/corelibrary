using System;
using Autofac;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.PdfRocket;

public class PdfRocketModule : AppModule
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddHttpClient<PdfRocketGenerator>(c => c.BaseAddress = new Uri(PdfRocketGenerator.ApiUrl));
}
