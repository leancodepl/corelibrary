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
        AndroidVersionsConfiguration androidConfiguration,
        IOSVersionsConfiguration iOSConfiguration
    )
    {
        cqrsServicesBuilder.Services.AddTransient<VersionHandler>();

        cqrsServicesBuilder.Services.AddSingleton(androidConfiguration);

        cqrsServicesBuilder.Services.AddSingleton(iOSConfiguration);

        return cqrsServicesBuilder.AddCQRSObjects(
            TypesCatalog.Of<VersionSupport>(),
            TypesCatalog.Of<VersionSupportQH>()
        );
    }
}
