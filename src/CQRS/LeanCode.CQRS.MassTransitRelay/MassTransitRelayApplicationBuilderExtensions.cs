using LeanCode.CQRS.MassTransitRelay.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.CQRS.MassTransitRelay;

public static class MassTransitRelayApplicationBuilderExtensions
{
    public static IApplicationBuilder CommitTransaction<TDbContext>(this IApplicationBuilder builder)
        where TDbContext : DbContext
    {
        builder.UseMiddleware<CommitDatabaseTransactionMiddleware<TDbContext>>();
        return builder;
    }

    public static IApplicationBuilder PublishEvents(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<EventsPublisherMiddleware>();
        return builder;
    }
}
