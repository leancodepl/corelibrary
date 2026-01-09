using Semver;

namespace LeanCode.ForceUpdate;

public sealed record IOSVersionsConfiguration(SemVersion MinimumRequiredVersion, SemVersion CurrentlySupportedVersion)
{
    [Obsolete("Switch to explicit SemVersion instead")]
    public IOSVersionsConfiguration(Version minimumRequiredVersion, Version currentlySupportedVersion)
        : this(
            SemVersion.Parse(minimumRequiredVersion.ToString(), SemVersionStyles.Any),
            SemVersion.Parse(currentlySupportedVersion.ToString(), SemVersionStyles.Any)
        ) { }
}

public sealed record AndroidVersionsConfiguration(
    SemVersion MinimumRequiredVersion,
    SemVersion CurrentlySupportedVersion
)
{
    [Obsolete("Switch to explicit SemVersion instead")]
    public AndroidVersionsConfiguration(Version minimumRequiredVersion, Version currentlySupportedVersion)
        : this(
            SemVersion.Parse(minimumRequiredVersion.ToString(), SemVersionStyles.Any),
            SemVersion.Parse(currentlySupportedVersion.ToString(), SemVersionStyles.Any)
        ) { }
}
