using System;
using LeanCode.CQRS.Default.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.Example
{
    public sealed class AppContext : DefaultExecutionContext
    {
        public Guid UserId { get; }
        public string Header { get; }

        public AppContext(Guid userId, string header)
        {
            UserId = userId;
            Header = header;
        }

        public static AppContext FromHttp(HttpContext context)
        {
            Guid.TryParse(context.User?.FindFirst("sub")?.Value, out var uid); ;
            var header = context.Request.Headers["X-Example"];
            return new AppContext(uid, header);
        }
    }
}
