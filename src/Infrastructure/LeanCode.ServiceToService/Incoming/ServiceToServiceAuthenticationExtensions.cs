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
    /// <summary>Registers S2S authentication under the module scheme.</summary>
    /// <remarks>
    /// Security relies on infrastructure validating and enforcing S2S caller identity headers.
    /// </remarks>
    /// <param name="builder">The authentication builder.</param>
    /// <param name="configureOptions">The S2S options configuration callback.</param>
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

    /// <summary>Registers a policy scheme that selects default or S2S auth.</summary>
    /// <remarks>
    /// The policy selects S2S authentication when <c>LNCD-S2S-Api-Key</c> header is present.
    /// </remarks>
    /// <param name="builder">The authentication builder.</param>
    /// <param name="defaultScheme">The fallback scheme when S2S key header is missing.</param>
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
