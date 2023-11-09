using LeanCode.Contracts;
using LeanCode.Contracts.Security;

namespace LeanCode.AppRating.Contracts;

[AuthorizeWhenHasAnyOf(Permissions.RateApp)]
public class SubmitAppRating : ICommand
{
    public double Rating { get; set; }
    public string? AdditionalComment { get; set; }
    public PlatformDTO Platform { get; set; }
    public string SystemVersion { get; set; }
    public string AppVersion { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public enum PlatformDTO
{
    Android = 0,
    IOS = 1,
}
