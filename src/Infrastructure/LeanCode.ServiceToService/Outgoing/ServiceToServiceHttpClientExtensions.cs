using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.ServiceToService.Outgoing;

public static class ServiceToServiceHttpClientExtensions
{
    public static IHttpClientBuilder AddCallerIdentity(this IHttpClientBuilder builder, string serviceName)
    {
        return builder.ConfigureHttpClient(client =>
            client.DefaultRequestHeaders.Add(ServiceToServiceDefaults.CallerIdHeaderName, serviceName)
        );
    }
}
