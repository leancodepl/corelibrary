using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    sealed class RemoteCQRSMiddleware
    {
        private readonly RemoteCommandHandler commandHandler;
        private readonly RemoteQueryHandler queryHandler;

        public RemoteCQRSMiddleware(RemoteCommandHandler commandHandler, RemoteQueryHandler queryHandler)
        {
            this.commandHandler = commandHandler;
            this.queryHandler = queryHandler;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            IActionResult actionResult;
            if (request.Method != HttpMethods.Post)
            {
                actionResult = new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }
            else if (request.Path.StartsWithSegments("/query"))
            {
                actionResult = await queryHandler.ExecuteAsync(request).ConfigureAwait(false);
            }
            else if (request.Path.StartsWithSegments("/command"))
            {
                actionResult = await commandHandler.ExecuteAsync(request).ConfigureAwait(false);
            }
            else
            {
                actionResult = new StatusCodeResult(StatusCodes.Status404NotFound);
            }

            actionResult.Execute(context);
        }
    }

    public static class RemoteCQRSMiddlewareExtensions
    {
        public static IApplicationBuilder UseRemoteCQRS(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RemoteCQRSMiddleware>();
        }
    }
}
