using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.AuditLogs;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder Audit<TDbContext>(this IApplicationBuilder builder)
        where TDbContext : DbContext
    {
        builder.UseMiddleware<AuditLogsMiddleware<TDbContext>>();
        return builder;
    }
}
