using Microsoft.AspNetCore.Http;

namespace LeanCode.ForceUpdate.Services;

public class VersionHandler
{
    public virtual Task<VersionSupportResult> CheckVersionAsync(
        Version version,
        Version minimumRequiredVersion,
        Version currentlySupportedVersion,
        HttpContext context
    )
    {
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

    public enum VersionSupportResult
    {
        UpdateRequired,
        UpdateSuggested,
        UpToDate,
    }
}
