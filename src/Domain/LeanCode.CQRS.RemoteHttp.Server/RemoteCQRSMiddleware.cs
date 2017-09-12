using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    public sealed class RemoteCQRSMiddleware<TAppContext>
    {
        public RemoteCQRSMiddleware(RequestDelegate next)
        { }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            ActionResult actionResult;
            if (request.Method != HttpMethods.Post)
            {
                actionResult = new ActionResult.StatusCode(StatusCodes.Status405MethodNotAllowed);
            }
            else if (request.Path.StartsWithSegments("/query"))
            {
                var queryHandler = context.RequestServices.GetService<IRemoteQueryHandler<TAppContext>>();
                actionResult = await queryHandler.ExecuteAsync(context).ConfigureAwait(false);
            }
            else if (request.Path.StartsWithSegments("/command"))
            {
                var commandHandler = context.RequestServices.GetService<IRemoteCommandHandler<TAppContext>>();
                actionResult = await commandHandler.ExecuteAsync(context).ConfigureAwait(false);
            }
            else
            {
                actionResult = new ActionResult.StatusCode(StatusCodes.Status404NotFound);
            }

            actionResult.Execute(context);
        }
    }

    public static class RemoteCQRSMiddlewareExtensions
    {
        public static IApplicationBuilder UseRemoteCQRS<TAppContext>(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RemoteCQRSMiddleware<TAppContext>>();
        }
    }
}
