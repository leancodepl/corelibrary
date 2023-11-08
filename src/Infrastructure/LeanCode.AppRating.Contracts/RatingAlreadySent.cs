using LeanCode.Contracts;
using LeanCode.Contracts.Security;

namespace LeanCode.AppRating.Contracts;

[AuthorizeWhenHasAnyOf(Permissions.RateApp)]
public class RatingAlreadySent : IQuery<bool> { }
