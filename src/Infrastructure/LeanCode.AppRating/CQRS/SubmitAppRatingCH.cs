using FluentValidation;
using LeanCode.AppRating.Contracts;
using LeanCode.AppRating.DataAccess;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation.Fluent;
using LeanCode.TimeProvider;
using Microsoft.AspNetCore.Http;

namespace LeanCode.AppRating.CQRS;

public class SubmitAppRatingCV : AbstractValidator<SubmitAppRating>
{
    public SubmitAppRatingCV()
    {
        RuleFor(cmd => cmd.Rating).InclusiveBetween(1, 5).WithCode(SubmitAppRating.ErrorCodes.RatingInvalid);

        RuleFor(cmd => cmd.Platform).IsInEnum().WithCode(SubmitAppRating.ErrorCodes.PlatformInvalid);

        RuleFor(cmd => cmd.AdditionalComment)
            .MaximumLength(4000)
            .WithCode(SubmitAppRating.ErrorCodes.AdditionalCommentTooLong);
        RuleFor(cmd => cmd.SystemVersion)
            .NotEmpty()
            .WithCode(SubmitAppRating.ErrorCodes.SystemVersionRequired)
            .MaximumLength(200)
            .WithCode(SubmitAppRating.ErrorCodes.SystemVersionTooLong);
        RuleFor(cmd => cmd.AppVersion)
            .NotEmpty()
            .WithCode(SubmitAppRating.ErrorCodes.AppVersionRequired)
            .MaximumLength(200)
            .WithCode(SubmitAppRating.ErrorCodes.AppVersionTooLong);
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
                    Time.NowWithOffset,
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
