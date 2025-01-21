namespace LeanCode.ConfigCat;

public sealed record class ConfigCatOptions(
    string? SdkKey,
    string? FlagOverridesFilePath,
    string? FlagOverridesJsonObject
)
{
    public ConfigCatOptions()
        : this(default, default, default) { }
};
