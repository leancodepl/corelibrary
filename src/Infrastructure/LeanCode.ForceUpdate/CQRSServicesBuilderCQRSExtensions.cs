using LeanCode.Components;
using LeanCode.CQRS.AspNetCore;
using LeanCode.ForceUpdate.Contracts;
using LeanCode.ForceUpdate.Services.CQRS;
using LeanCode.ForceUpdate.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.ForceUpdate;

public static class CQRSServicesBuilderCQRSExtensions
{
    public static CQRSServicesBuilder AddForceUpdate(this CQRSServicesBuilder cqrsServicesBuilder)
    {
        cqrsServicesBuilder.services.AddTransient<VersionHandler>();
        return cqrsServicesBuilder.AddCQRSObjects(
            TypesCatalog.Of<VersionSupport>(),
            TypesCatalog.Of<VersionSupportQH>()
        );
    }
}
