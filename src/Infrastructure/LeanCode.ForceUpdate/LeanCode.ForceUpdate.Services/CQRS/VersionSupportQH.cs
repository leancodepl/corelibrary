using LeanCode.CQRS.Execution;
using LeanCode.ForceUpdate.Contracts;
using Microsoft.AspNetCore.Http;
using VersionSupport = LeanCode.ForceUpdate.Contracts.VersionSupport;

namespace LeanCode.ForceUpdate.Services.CQRS;

public class VersionSupportQH : IQueryHandler<VersionSupport, VersionSupportDTO?>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<VersionSupportQH>();

    private readonly AndroidVersionsConfiguration androidConfiguration;
    private readonly IOSVersionsConfiguration iOSConfiguration;
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

    public async Task<VersionSupportDTO?> ExecuteAsync(HttpContext context, VersionSupport query)
    {
        if (!Version.TryParse(query.Version, out var version) || !Enum.IsDefined<PlatformDTO>(query.Platform))
        {
            logger.Warning("Invalid input: {Version}, {Platform}", query.Version, query.Platform);
            return null;
        }

        return query.Platform switch
        {
            PlatformDTO.IOS
                => new VersionSupportDTO
                {
                    CurrentlySupportedVersion = iOSConfiguration.CurrentlySupportedVersion.ToString(),
                    MinimumRequiredVersion = iOSConfiguration.MinimumRequiredVersion.ToString(),
                    Result = (VersionSupportResultDTO)(
                        await versionHandler.CheckVersionAsync(version, (Platform)query.Platform, context)
                    ),
                },
            PlatformDTO.Android
                => new VersionSupportDTO
                {
                    CurrentlySupportedVersion = androidConfiguration.CurrentlySupportedVersion.ToString(),
                    MinimumRequiredVersion = androidConfiguration.MinimumRequiredVersion.ToString(),
                    Result = (VersionSupportResultDTO)(
                        await versionHandler.CheckVersionAsync(version, (Platform)query.Platform, context)
                    ),
                },
            _ => throw new InvalidOperationException($"Invalid platform: {query.Platform}."),
        };
    }
}
