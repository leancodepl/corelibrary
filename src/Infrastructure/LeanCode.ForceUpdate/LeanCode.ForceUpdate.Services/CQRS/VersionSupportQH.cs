using LeanCode.CQRS.Execution;
using LeanCode.ForceUpdate.Contracts;
using Microsoft.AspNetCore.Http;
using VersionSupport = LeanCode.ForceUpdate.Contracts.VersionSupport;

namespace LeanCode.ForceUpdate.Services.CQRS;

public class VersionSupportQH : IQueryHandler<VersionSupport, VersionSupportDTO?>
{
    private readonly IOSVersionsConfiguration iOSConfiguration;
    private readonly AndroidVersionsConfiguration androidConfiguration;
    private readonly VersionHandler versionHandler;

    public VersionSupportQH(
        IOSVersionsConfiguration iOSConfiguration,
        AndroidVersionsConfiguration androidConfiguration,
        VersionHandler versionHandler
    )
    {
        this.iOSConfiguration = iOSConfiguration;
        this.androidConfiguration = androidConfiguration;
        this.versionHandler = versionHandler;
    }

    public Task<VersionSupportDTO?> ExecuteAsync(HttpContext context, VersionSupport query)
    {
        if (!Version.TryParse(query.Version, out var version) || !Enum.IsDefined<PlatformDTO>(query.Platform))
        {
            return Task.FromResult<VersionSupportDTO?>(null);
        }

        var result = query.Platform switch
        {
            PlatformDTO.IOS
                => versionHandler.CheckVersion(
                    version,
                    iOSConfiguration.MinimumRequiredVersion,
                    iOSConfiguration.CurrentlySupportedVersion,
                    context
                ),
            PlatformDTO.Android
                => versionHandler.CheckVersion(
                    version,
                    androidConfiguration.MinimumRequiredVersion,
                    androidConfiguration.CurrentlySupportedVersion,
                    context
                ),
            _ => throw new InvalidOperationException("Invalid platform"),
        };

        return Task.FromResult((VersionSupportDTO?)result);
    }
}
