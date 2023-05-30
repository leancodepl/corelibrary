using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.PdfRocket;

public static class PdfRocketServiceCollectionExtensions
{
    public static IServiceCollection AddPdfRocket(this IServiceCollection services, PdfRocketConfiguration config)
    {
        services.AddSingleton(config);
        services.AddHttpClient<PdfRocketGenerator>(c => c.BaseAddress = new Uri(PdfRocketGenerator.ApiUrl));
        return services;
    }
}
