using LeanCode.AppRating.Contracts;
using LeanCode.AppRating.DataAccess;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.AppRating.CQRS;

public class RatingAlreadySentQH<TUserId> : IQueryHandler<RatingAlreadySent, bool>
    where TUserId : notnull, IEquatable<TUserId>
{
    private readonly IAppRatingStore<TUserId> store;
    private readonly IUserIdExtractor<TUserId> extractor;

    public RatingAlreadySentQH(IAppRatingStore<TUserId> store, IUserIdExtractor<TUserId> extractor)
    {
        this.store = store;
        this.extractor = extractor;
    }

    public Task<bool> ExecuteAsync(HttpContext context, RatingAlreadySent query)
    {
        return store
            .AppRatings
            .Where(r => (object)r.UserId == (object)extractor.Extract(context))
            .AnyAsync(context.RequestAborted);
    }
}
