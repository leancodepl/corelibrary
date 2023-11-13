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

    public static class ErrorCodes
    {
        public const int RatingInvalid = 1;
        public const int AdditionalCommentTooLong = 2;
        public const int PlatformInvalid = 3;
        public const int SystemVersionRequired = 4;
        public const int SystemVersionTooLong = 5;
        public const int AppVersionRequired = 6;
        public const int AppVersionTooLong = 7;
    }
}

public enum PlatformDTO
{
    Android = 0,
    IOS = 1,
}
