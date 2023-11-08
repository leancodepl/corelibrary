using LeanCode.Contracts;
using LeanCode.Contracts.Security;

namespace LeanCode.AppRating.Contracts;

[AuthorizeWhenHasAnyOf(Permissions.RateApp)]
public class SubmitAppRating : ICommand
{
    public double Rating { get; set; }
    public string? AdditionalComment { get; set; }
}
