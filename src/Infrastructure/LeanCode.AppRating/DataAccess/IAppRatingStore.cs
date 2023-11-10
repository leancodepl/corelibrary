using Microsoft.EntityFrameworkCore;

namespace LeanCode.AppRating.DataAccess;

public interface IAppRatingStore<TUserId>
    where TUserId : notnull, IEquatable<TUserId>
{
    public DbSet<AppRatingEntity<TUserId>> AppRatings { get; }
}
