using LeanCode.Components;
using LeanCode.CQRS.AspNetCore;
using LeanCode.ForceUpdate.Contracts;
using LeanCode.ForceUpdate.Services.CQRS;
using LeanCode.ForceUpdate.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.ForceUpdate;

public static class CQRSServicesBuilderCQRSExtensions
{
    public static CQRSServicesBuilder AddForceUpdate(
        this CQRSServicesBuilder cqrsServicesBuilder,
        Version androidMinimumRequiredVersion,
        Version androidCurrentlySupportedVersion,
        Version iosMinimumRequiredVersion,
        Version iosCurrentlySupportedVersion
    )
    {
        cqrsServicesBuilder.Services.AddTransient<VersionHandler>();

        cqrsServicesBuilder.Services.AddSingleton(
            new AndroidVersionsConfiguration(androidMinimumRequiredVersion, androidCurrentlySupportedVersion)
        );

        cqrsServicesBuilder.Services.AddSingleton(
            new IOSVersionsConfiguration(iosMinimumRequiredVersion, iosCurrentlySupportedVersion)
        );

        return cqrsServicesBuilder.AddCQRSObjects(
            TypesCatalog.Of<VersionSupport>(),
            TypesCatalog.Of<VersionSupportQH>()
        );
    }
}
