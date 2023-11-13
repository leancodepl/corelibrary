using LeanCode.AppRating.Contracts;
using LeanCode.AppRating.CQRS;
using LeanCode.AppRating.DataAccess;
using LeanCode.Components;
using LeanCode.CQRS.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.AppRating;

public static class CQRSServicesBuilderExtensions
{
    public static CQRSServicesBuilder AddAppRating<TUserId, TDbContext>(
        this CQRSServicesBuilder cqrsServicesBuilder,
        IUserIdExtractor<TUserId> extractor
    )
        where TDbContext : DbContext, IAppRatingStore<TUserId>
        where TUserId : notnull, IEquatable<TUserId>
    {
        cqrsServicesBuilder.Services.AddTransient<IAppRatingStore<TUserId>>(sp => sp.GetRequiredService<TDbContext>());
        cqrsServicesBuilder.Services.AddSingleton(extractor);

        return cqrsServicesBuilder.AddCQRSObjects(
            TypesCatalog.Of<SubmitAppRating>(),
            TypesCatalog.Of<SubmitAppRatingCH<TUserId>>()
        );
    }
}
