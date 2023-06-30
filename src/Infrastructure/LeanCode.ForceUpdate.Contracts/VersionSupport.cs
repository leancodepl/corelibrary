using LeanCode.Contracts;

namespace LeanCode.ForceUpdate.Contracts;

public class VersionSupport : IQuery<VersionSupportDTO>
{
    public PlatformDTO Platform { get; set; }
    public string Version { get; set; }
}

public class VersionSupportDTO
{
    public string CurrentlySupportedVersion { get; set; }
    public string MinimumRequiredVersion { get; set; }
    public VersionSupportResultDTO Result { get; set; }
}

public enum PlatformDTO
{
    Android = 0,
    IOS = 1,
}

public enum VersionSupportResultDTO
{
    UpdateRequired = 0,
    UpdateSuggested = 1,
    UpToDate = 2,
}
