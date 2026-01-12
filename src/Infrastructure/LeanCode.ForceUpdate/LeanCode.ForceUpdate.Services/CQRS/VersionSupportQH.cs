using LeanCode.CQRS.Execution;
using LeanCode.ForceUpdate.Contracts;
using LeanCode.Logging;
using Microsoft.AspNetCore.Http;
using Semver;
using VersionSupport = LeanCode.ForceUpdate.Contracts.VersionSupport;

namespace LeanCode.ForceUpdate.Services.CQRS;

public class VersionSupportQH : IQueryHandler<VersionSupport, VersionSupportDTO>
{
    private readonly AndroidVersionsConfiguration androidConfiguration;
    private readonly IOSVersionsConfiguration iOSConfiguration;
    private readonly VersionHandler versionHandler;
    private readonly ILogger<VersionSupportQH> logger;

    public VersionSupportQH(
        IOSVersionsConfiguration iOSConfiguration,
        AndroidVersionsConfiguration androidConfiguration,
        VersionHandler versionHandler,
        ILogger<VersionSupportQH> logger
    )
    {
        this.iOSConfiguration = iOSConfiguration;
        this.androidConfiguration = androidConfiguration;
        this.versionHandler = versionHandler;
        this.logger = logger;
    }

    public async Task<VersionSupportDTO> ExecuteAsync(HttpContext context, VersionSupport query)
    {
        if (
            !SemVersion.TryParse(query.Version, SemVersionStyles.Any, out var version)
            || !Enum.IsDefined(query.Platform)
        )
        {
            logger.Warning("Invalid input: {Version}, {Platform}", query.Version, query.Platform);
            return new VersionSupportDTO
            {
                CurrentlySupportedVersion = "0.0.0",
                MinimumRequiredVersion = "0.0.0",
                Result = VersionSupportResultDTO.UpToDate,
            };
        }
        else
        {
            var (minimum, current) = GetVersions(query.Platform);
            return new VersionSupportDTO
            {
                CurrentlySupportedVersion = current.ToString(),
                MinimumRequiredVersion = minimum.ToString(),
                Result = await versionHandler.CheckVersionAsync(version, query.Platform, context),
            };
        }
    }

    private (SemVersion Minimum, SemVersion Current) GetVersions(PlatformDTO platform)
    {
        return platform switch
        {
            PlatformDTO.Android => (
                androidConfiguration.MinimumRequiredVersion,
                androidConfiguration.CurrentlySupportedVersion
            ),
            PlatformDTO.IOS => (iOSConfiguration.MinimumRequiredVersion, iOSConfiguration.CurrentlySupportedVersion),
            _ => throw new InvalidOperationException($"Invalid platform: {platform}."),
        };
    }
}
