using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace LeanCode.ServiceToService.Incoming;

public static class ServiceToServiceAuthenticationExtensions
{
    public static AuthenticationBuilder AddServiceToService(
        this AuthenticationBuilder builder,
        Action<ServiceToServiceAuthenticationOptions> configureOptions
    )
    {
        return builder.AddServiceToService(ServiceToServiceDefaults.AuthenticationScheme, configureOptions);
    }

    public static AuthenticationBuilder AddServiceToService(
        this AuthenticationBuilder builder,
        string authenticationScheme,
        Action<ServiceToServiceAuthenticationOptions> configureOptions
    )
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IValidateOptions<ServiceToServiceAuthenticationOptions>,
                ServiceToServiceCallerRolesValidator
            >()
        );
        builder.Services.AddOptions<ServiceToServiceAuthenticationOptions>(authenticationScheme).ValidateOnStart();

        return builder.AddScheme<ServiceToServiceAuthenticationOptions, ServiceToServiceAuthenticationHandler>(
            authenticationScheme,
            configureOptions
        );
    }

    public static AuthenticationBuilder AddServiceToServicePolicyScheme(
        this AuthenticationBuilder builder,
        string defaultScheme,
        string policyScheme = ServiceToServiceDefaults.PolicyScheme
    )
    {
        return builder.AddPolicyScheme(
            policyScheme,
            policyScheme,
            schemeOptions =>
            {
                schemeOptions.ForwardDefaultSelector = context =>
                {
                    if (
                        !context.Request.Headers.TryGetValue(
                            ServiceToServiceDefaults.S2SApiKeyHeaderName,
                            out var s2SApiKeyValues
                        ) || string.IsNullOrWhiteSpace(s2SApiKeyValues.ToString())
                    )
                    {
                        return defaultScheme;
                    }

                    return ServiceToServiceDefaults.AuthenticationScheme;
                };
            }
        );
    }
}
