namespace LeanCode.ConfigCat;

public sealed record ConfigCatOptions(string? SdkKey, string? FlagOverridesFilePath, string? FlagOverridesJsonObject)
{
    public ConfigCatOptions()
        : this(default, default, default) { }
};
