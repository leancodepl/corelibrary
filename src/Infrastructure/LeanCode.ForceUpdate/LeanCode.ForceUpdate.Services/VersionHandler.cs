using Microsoft.AspNetCore.Http;
using LeanCode.ForceUpdate.Contracts;

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
        Version version,
        PlatformDTO platform,
        HttpContext context
    )
    {
        var (minimumRequiredVersion, currentlySupportedVersion) = platform switch
        {
            PlatformDTO.Android
                => (androidConfiguration.MinimumRequiredVersion, androidConfiguration.CurrentlySupportedVersion),
            PlatformDTO.IOS => (iOSConfiguration.MinimumRequiredVersion, iOSConfiguration.CurrentlySupportedVersion),
            _ => throw new InvalidOperationException($"Invalid platform: {platform}."),
        };

        if (version < minimumRequiredVersion)
        {
            return ValueTask.FromResult(VersionSupportResultDTO.UpdateRequired);
        }
        else if (version >= minimumRequiredVersion && version < currentlySupportedVersion)
        {
            return ValueTask.FromResult(VersionSupportResultDTO.UpdateSuggested);
        }
        else
        {
            return ValueTask.FromResult(VersionSupportResultDTO.UpToDate);
        }
    }
}
