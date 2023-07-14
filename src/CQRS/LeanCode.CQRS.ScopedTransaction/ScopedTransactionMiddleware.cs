using System.Diagnostics.CodeAnalysis;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.CQRS.ScopedTransaction;

public class ScopedTransactionMiddleware<TDbContext>
    where TDbContext : DbContext
{
    private readonly RequestDelegate next;

    public ScopedTransactionMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public Task InvokeAsync(HttpContext httpContext, TDbContext dbContext)
    {
        return UsePessimisticConcurrency(httpContext, out var config)
            ? RunWithPessimisticConcurrency(httpContext, dbContext, config)
            : RunWithOptimisticConcurrency(httpContext, dbContext);
    }

    private static bool UsePessimisticConcurrency(
        HttpContext httpContext,
        [NotNullWhen(true)] out PessimisticConcurrencyAttribute? config
    )
    {
        var cqrsMetadata = httpContext.GetCQRSEndpoint().ObjectMetadata;
        var handlerType = cqrsMetadata.HandlerType;

        if (
            Attribute.GetCustomAttribute(handlerType, typeof(PessimisticConcurrencyAttribute))
            is PessimisticConcurrencyAttribute attr
        )
        {
            config = attr;
            return true;
        }
        else
        {
            config = null;
            return false;
        }
    }

    private async Task RunWithOptimisticConcurrency(HttpContext httpContext, TDbContext dbContext)
    {
        await next(httpContext);
        await dbContext.SaveChangesAsync(httpContext.RequestAborted);
    }

    private async Task RunWithPessimisticConcurrency(
        HttpContext httpContext,
        TDbContext dbContext,
        PessimisticConcurrencyAttribute config
    )
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(config.IsolationLevel);

        try
        {
            await next(httpContext);

            if (config.SaveChanges)
            {
                await dbContext.SaveChangesAsync(httpContext.RequestAborted);
            }

            await transaction.CommitAsync(httpContext.RequestAborted);
        }
        catch
        {
            await transaction.RollbackAsync(httpContext.RequestAborted);
            throw;
        }
    }
}

public static class IApplicationBuilderExtensions
{
    public static IApplicationBuilder CommitTransaction<TDbContext>(this IApplicationBuilder builder)
        where TDbContext : DbContext
    {
        return builder.UseMiddleware<ScopedTransactionMiddleware<TDbContext>>();
    }
}
