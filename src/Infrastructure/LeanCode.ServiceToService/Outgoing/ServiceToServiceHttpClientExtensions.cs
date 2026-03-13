using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.ServiceToService.Outgoing;

public static class ServiceToServiceHttpClientExtensions
{
    public static IHttpClientBuilder AddCallerIdentity(
        this IHttpClientBuilder builder,
        string serviceName,
        string s2SApiKey,
        string s2SApiKeyHeaderName = ServiceToServiceDefaults.S2SApiKeyHeaderName
    )
    {
        return builder.ConfigureHttpClient(client =>
        {
            client.DefaultRequestHeaders.Add(ServiceToServiceDefaults.CallerIdHeaderName, serviceName);
            client.DefaultRequestHeaders.Add(s2SApiKeyHeaderName, s2SApiKey);
        });
    }
}
