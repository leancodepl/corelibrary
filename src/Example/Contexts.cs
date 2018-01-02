using System;
using LeanCode.CQRS.Default.Execution;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.Example
{
    public sealed class AppContext : DefaultExecutionContext
    {
        public string CustomHeaderValue { get; private set; }

        public static AppContext FromHttp(HttpContext context)
        {
            return new AppContext
            {
                User = context.User,
                CustomHeaderValue = context.Request.Headers["X-Example"]
            };
        }
    }

    public sealed class LocalContext
    {
        public Guid UserId { get; }
        public string Header { get; }

        public LocalContext()
        { }

        public LocalContext(Guid userId, string header)
        {
            UserId = userId;
            Header = header;
        }

        public static LocalContext Empty()
        {
            return new LocalContext(Guid.Empty, string.Empty);
        }
    }

    public class LocalContextFromAppContextFactory
        : IObjectContextFromAppContextFactory<AppContext, LocalContext>
    {
        public LocalContext Create(AppContext appContext)
        {
            Guid.TryParse(appContext.User?.FindFirst("sub")?.Value, out var uid);
            return new LocalContext(uid, appContext.CustomHeaderValue);
        }
    }
}
