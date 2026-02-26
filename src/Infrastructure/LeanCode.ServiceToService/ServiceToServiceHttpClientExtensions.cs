using LeanCode.ServiceToService;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceToServiceHttpClientExtensions
{
    public static IHttpClientBuilder AddCallerIdentity(this IHttpClientBuilder builder, string serviceName)
    {
        return builder.ConfigureHttpClient(client =>
            client.DefaultRequestHeaders.Add(ServiceToServiceConstants.CallerIdHeaderName, serviceName)
        );
    }
}
