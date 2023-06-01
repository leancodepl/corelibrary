using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.CQRS.MassTransitRelay.Middleware;

public class CommitDatabaseTransactionMiddleware<TDbContext>
    where TDbContext : DbContext
{
    private readonly RequestDelegate next;

    public CommitDatabaseTransactionMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, TDbContext dbContext)
    {
        await next(httpContext);
        await dbContext.SaveChangesAsync(httpContext.RequestAborted);
    }
}
