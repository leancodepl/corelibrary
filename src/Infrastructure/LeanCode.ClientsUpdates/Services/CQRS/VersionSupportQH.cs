using LeanCode.CQRS.Execution;
using LeanCode.ClientsUpdates.Contracts;
using Microsoft.AspNetCore.Http;
using VersionSupport = LeanCode.ClientsUpdates.Contracts.VersionSupport;

namespace LeanCode.ClientsUpdates.Services.CQRS;

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
        if (!Version.TryParse(query.Version, out var version) || !Enum.IsDefined(typeof(PlatformDTO), query.Platform))
        {
            return Task.FromResult<VersionSupportDTO?>(null);
        }

        return query.Platform switch
        {
            PlatformDTO.IOS
                => Task.FromResult<VersionSupportDTO?>(
                    (VersionSupportDTO)
                        versionHandler.CheckVersion(
                            version,
                            iOSConfiguration.MinimumRequiredVersion,
                            iOSConfiguration.CurrentlySupportedVersion,
                            context
                        )
                ),
            PlatformDTO.Android
                => Task.FromResult<VersionSupportDTO?>(
                    (VersionSupportDTO)
                        versionHandler.CheckVersion(
                            version,
                            iOSConfiguration.MinimumRequiredVersion,
                            iOSConfiguration.CurrentlySupportedVersion,
                            context
                        )
                ),
            _ => throw new InvalidOperationException("Invalid platform"),
        };
    }
}
