using LeanCode.ServiceToService;
using LeanCode.ServiceToService.Incoming;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "?",
    "IDE0130:NamespaceDoesNotMatchFolderStructure",
    Justification = "Extensions on AuthenticationBuilder by convention live in Microsoft.Extensions.DependencyInjection namespace.",
    Scope = "namespace",
    Target = "~N:Microsoft.Extensions.DependencyInjection"
)]

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceToServiceAuthenticationExtensions
{
    public static AuthenticationBuilder AddServiceToService(
        this AuthenticationBuilder builder,
        Action<ServiceToServiceAuthenticationOptions> configureOptions
    )
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IValidateOptions<ServiceToServiceAuthenticationOptions>,
                ServiceToServiceCallerRolesValidator
            >()
        );
        builder
            .Services.AddOptions<ServiceToServiceAuthenticationOptions>(ServiceToServiceConsts.AuthenticationScheme)
            .ValidateOnStart();

        return builder.AddScheme<ServiceToServiceAuthenticationOptions, ServiceToServiceAuthenticationHandler>(
            ServiceToServiceConsts.AuthenticationScheme,
            configureOptions
        );
    }

    public static AuthenticationBuilder AddServiceToServicePolicyScheme(
        this AuthenticationBuilder builder,
        string defaultScheme
    )
    {
        return builder.AddPolicyScheme(
            ServiceToServiceConsts.PolicyScheme,
            ServiceToServiceConsts.PolicyScheme,
            schemeOptions =>
            {
                schemeOptions.ForwardDefaultSelector = context =>
                {
                    if (
                        !context.Request.Headers.TryGetValue(
                            ServiceToServiceConsts.S2SApiKeyHeaderName,
                            out var s2SApiKeyValues
                        ) || string.IsNullOrWhiteSpace(s2SApiKeyValues.ToString())
                    )
                    {
                        return defaultScheme;
                    }

                    return ServiceToServiceConsts.AuthenticationScheme;
                };
            }
        );
    }
}
