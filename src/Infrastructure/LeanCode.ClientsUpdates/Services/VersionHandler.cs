using Microsoft.AspNetCore.Http;

namespace LeanCode.ClientsUpdates.Services;

public class VersionHandler
{
    public virtual VersionSupport CheckVersion(
        Version version,
        Version minimumRequiredVersion,
        Version currentlySupportedVersion,
        HttpContext context
    )
    {
        if (version < minimumRequiredVersion)
        {
            return VersionSupport.UpdateRequired;
        }
        else if (version >= minimumRequiredVersion && version < currentlySupportedVersion)
        {
            return VersionSupport.UpdateSuggested;
        }
        else
        {
            return VersionSupport.UpToDate;
        }
    }

    public enum VersionSupport
    {
        UpdateRequired,
        UpdateSuggested,
        UpToDate,
    }
}
