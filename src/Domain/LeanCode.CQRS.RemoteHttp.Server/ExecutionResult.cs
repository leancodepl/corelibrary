using System.IO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    abstract class ActionResult
    {
        public abstract void Execute(HttpContext ctx);

        public sealed class StatusCode : ActionResult
        {
            public int Code { get; }

            public StatusCode(int statusCode)
            {
                Code = statusCode;
            }

            public override void Execute(HttpContext ctx)
            {
                ctx.Response.StatusCode = Code;
            }
        }

        public sealed class Json : ActionResult
        {
            public object Payload { get; }
            public int Code { get; }

            public Json(object payload, int statusCode)
            {
                Code = statusCode;
                Payload = payload;
            }

            public Json(object payload)
                : this(payload, StatusCodes.Status200OK)
            { }

            public override void Execute(HttpContext ctx)
            {
                ctx.Response.ContentType = "application/json";
                ctx.Response.StatusCode = Code;
                using (var writer = new StreamWriter(ctx.Response.Body))
                {
                    new JsonSerializer().Serialize(writer, Payload);
                }
            }
        }
    }

}
