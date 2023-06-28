using LeanCode.Contracts;

namespace LeanCode.ClientsUpdates.Contracts;

public class VersionSupport : IQuery<VersionSupportDTO?>
{
    public PlatformDTO Platform { get; set; }
    public string Version { get; set; } = default!;
}

public enum VersionSupportDTO
{
    UpdateRequired,
    UpdateSuggested,
    UpToDate,
}
