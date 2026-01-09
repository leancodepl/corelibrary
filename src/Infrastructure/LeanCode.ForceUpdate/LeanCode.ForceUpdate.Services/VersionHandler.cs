using LeanCode.ForceUpdate.Contracts;
using Microsoft.AspNetCore.Http;
using Semver;

namespace LeanCode.ForceUpdate.Services;

public class VersionHandler
{
    private readonly AndroidVersionsConfiguration androidConfiguration;
    private readonly IOSVersionsConfiguration iOSConfiguration;

    public VersionHandler(AndroidVersionsConfiguration androidConfiguration, IOSVersionsConfiguration iOSConfiguration)
    {
        this.androidConfiguration = androidConfiguration;
        this.iOSConfiguration = iOSConfiguration;
    }

    public virtual ValueTask<VersionSupportResultDTO> CheckVersionAsync(
        SemVersion version,
        PlatformDTO platform,
        HttpContext context
    )
    {
        var (minimumRequiredVersion, currentlySupportedVersion) = platform switch
        {
            PlatformDTO.Android => (
                androidConfiguration.MinimumRequiredVersion,
                androidConfiguration.CurrentlySupportedVersion
            ),
            PlatformDTO.IOS => (iOSConfiguration.MinimumRequiredVersion, iOSConfiguration.CurrentlySupportedVersion),
            _ => throw new InvalidOperationException($"Invalid platform: {platform}."),
        };

        if (SemVersion.ComparePrecedence(version, minimumRequiredVersion) < 0)
        {
            return ValueTask.FromResult(VersionSupportResultDTO.UpdateRequired);
        }
        else if (
            SemVersion.ComparePrecedence(version, minimumRequiredVersion) >= 0
            && SemVersion.ComparePrecedence(version, currentlySupportedVersion) < 0
        )
        {
            return ValueTask.FromResult(VersionSupportResultDTO.UpdateSuggested);
        }
        else
        {
            return ValueTask.FromResult(VersionSupportResultDTO.UpToDate);
        }
    }
}
