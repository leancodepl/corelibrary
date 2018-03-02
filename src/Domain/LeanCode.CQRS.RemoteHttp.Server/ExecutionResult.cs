using System.IO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    struct ExecutionResult
    {
        public bool SkipExecution { get; private set; }
        public int StatusCode { get; private set; }
        public object Payload { get; private set; }

        public static ExecutionResult Skip() => new ExecutionResult
        {
            SkipExecution = true
        };

        public static ExecutionResult Failed(int code) => new ExecutionResult
        {
            StatusCode = code,
            Payload = null
        };

        public static ExecutionResult Success(object payload, int code = 200) => new ExecutionResult
        {
            StatusCode = code,
            Payload = payload
        };
    }

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
