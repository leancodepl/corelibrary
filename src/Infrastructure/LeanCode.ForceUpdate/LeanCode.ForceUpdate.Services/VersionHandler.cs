using Microsoft.AspNetCore.Http;

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

    public virtual Task<VersionSupportResult> CheckVersionAsync(Version version, Platform platform, HttpContext context)
    {
        var (minimumRequiredVersion, currentlySupportedVersion) = platform switch
        {
            Platform.Android
                => (androidConfiguration.MinimumRequiredVersion, androidConfiguration.CurrentlySupportedVersion),
            Platform.IOS => (iOSConfiguration.MinimumRequiredVersion, iOSConfiguration.CurrentlySupportedVersion),
            _ => throw new InvalidOperationException($"Invalid platform: {platform}."),
        };

        if (version < minimumRequiredVersion)
        {
            return Task.FromResult(VersionSupportResult.UpdateRequired);
        }
        else if (version >= minimumRequiredVersion && version < currentlySupportedVersion)
        {
            return Task.FromResult(VersionSupportResult.UpdateSuggested);
        }
        else
        {
            return Task.FromResult(VersionSupportResult.UpToDate);
        }
    }
}

public enum Platform
{
    Android,
    IOS,
}

public enum VersionSupportResult
{
    UpdateRequired,
    UpdateSuggested,
    UpToDate,
}
