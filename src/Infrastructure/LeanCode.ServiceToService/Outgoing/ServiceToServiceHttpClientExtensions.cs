using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.ServiceToService.Outgoing;

public static class ServiceToServiceHttpClientExtensions
{
    public static IHttpClientBuilder AddServiceToServiceCallerIdentity(
        this IHttpClientBuilder builder,
        string serviceName,
        string s2SApiKey
    )
    {
        return builder.ConfigureHttpClient(client =>
        {
            client.DefaultRequestHeaders.Add(ServiceToServiceDefaults.CallerIdHeaderName, serviceName);
            client.DefaultRequestHeaders.Add(ServiceToServiceDefaults.S2SApiKeyHeaderName, s2SApiKey);
        });
    }
}
