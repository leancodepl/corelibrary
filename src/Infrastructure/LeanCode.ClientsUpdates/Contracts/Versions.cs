using LeanCode.Contracts;

namespace LeanCode.ClientsUpdates.Contracts;

public class Versions : IQuery<List<VersionsDTO>> { }

public class VersionsDTO
{
    public PlatformDTO Platform { get; set; }
    public string MinimumRequiredVersion { get; set; } = default!;
    public string CurrentlySupportedVersion { get; set; } = default!;
}
