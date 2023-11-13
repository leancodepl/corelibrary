using LeanCode.AppRating.Contracts;
using LeanCode.AppRating.CQRS;
using LeanCode.AppRating.DataAccess;
using LeanCode.Components;
using LeanCode.CQRS.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.AppRating;

public static class CQRSServicesBuilderExtensions
{
    public static CQRSServicesBuilder AddAppRating<TUserId, TDbContext, TUserIdExtractor>(
        this CQRSServicesBuilder cqrsServicesBuilder
    )
        where TDbContext : DbContext, IAppRatingStore<TUserId>
        where TUserId : notnull, IEquatable<TUserId>
        where TUserIdExtractor : class, IUserIdExtractor<TUserId>
    {
        cqrsServicesBuilder.Services.AddTransient<IAppRatingStore<TUserId>>(sp => sp.GetRequiredService<TDbContext>());
        cqrsServicesBuilder.Services.AddSingleton<TUserIdExtractor>();

        return cqrsServicesBuilder.AddCQRSObjects(
            TypesCatalog.Of<SubmitAppRating>(),
            TypesCatalog.Of<SubmitAppRatingCH<TUserId>>()
        );
    }
}
