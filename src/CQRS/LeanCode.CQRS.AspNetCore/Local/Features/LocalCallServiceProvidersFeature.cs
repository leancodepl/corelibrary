using Microsoft.AspNetCore.Http.Features;

namespace LeanCode.CQRS.AspNetCore.Local;

internal class LocalCallServiceProvidersFeature : IServiceProvidersFeature
{
    public IServiceProvider RequestServices { get; set; }

    public LocalCallServiceProvidersFeature(IServiceProvider requestServices)
    {
        RequestServices = requestServices;
    }
}
