using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.ServiceToService.Outgoing;

public static class ServiceToServiceHttpClientExtensions
{
    /// <summary>Adds S2S caller identity and API key headers to outgoing requests.</summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <param name="serviceName">The calling service identifier.</param>
    /// <param name="s2SApiKey">The S2S API key value.</param>
    public static IHttpClientBuilder AddServiceToServiceCallerIdentity(
        this IHttpClientBuilder builder,
        string serviceName,
        string s2SApiKey
    )
    {
        return builder.ConfigureHttpClient(client =>
        {
            client.DefaultRequestHeaders.Add(ServiceToServiceConsts.CallerIdHeaderName, serviceName);
            client.DefaultRequestHeaders.Add(ServiceToServiceConsts.S2SApiKeyHeaderName, s2SApiKey);
        });
    }
}
