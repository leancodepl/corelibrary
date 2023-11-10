using System.Security.Claims;
using FluentValidation;
using LeanCode.AppRating.Contracts;
using LeanCode.AppRating.DataAccess;
using LeanCode.CQRS.Execution;
using LeanCode.TimeProvider;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.AppRating.CQRS;

public class SubmitAppRatingCV : AbstractValidator<SubmitAppRating>
{
    public SubmitAppRatingCV()
    {
        RuleFor(cmd => cmd.Rating).InclusiveBetween(1, 5);

        RuleFor(cmd => cmd.AdditionalComment).MaximumLength(4000);
        RuleFor(cmd => cmd.SystemVersion).MaximumLength(200);
        RuleFor(cmd => cmd.AppVersion).MaximumLength(200);
    }
}

public class SubmitAppRatingCH<TUserId> : ICommandHandler<SubmitAppRating>
    where TUserId : notnull, IEquatable<TUserId>
{
    private readonly IAppRatingStore<TUserId> store;
    private readonly IUserIdExtractor<TUserId> extractor;

    public SubmitAppRatingCH(IAppRatingStore<TUserId> store, IUserIdExtractor<TUserId> extractor)
    {
        this.store = store;
        this.extractor = extractor;
    }

    public Task ExecuteAsync(HttpContext context, SubmitAppRating command)
    {
        store
            .AppRatings
            .Add(
                new AppRatingEntity<TUserId>(
                    extractor.Extract(context),
                    Time.Now,
                    command.Rating,
                    command.AdditionalComment,
                    (Platform)command.Platform,
                    command.SystemVersion,
                    command.AppVersion,
                    command.Metadata
                )
            );

        return Task.CompletedTask;
    }
}
