using System.IO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    interface IActionResult
    {
        void Execute(HttpContext ctx);
    }

    sealed class StatusCodeResult : IActionResult
    {
        public int StatusCode { get; }

        public StatusCodeResult(int statusCode)
        {
            StatusCode = statusCode;
        }

        public void Execute(HttpContext ctx)
        {
            ctx.Response.StatusCode = StatusCode;
        }
    }

    sealed class JsonResult : IActionResult
    {
        public object Payload { get; }
        public int StatusCode { get; }

        public JsonResult(object payload, int statusCode)
        {
            StatusCode = statusCode;
            Payload = payload;
        }

        public JsonResult(object payload)
            : this(payload, StatusCodes.Status200OK)
        { }

        public void Execute(HttpContext ctx)
        {
            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode = StatusCode;
            using (var writer = new StreamWriter(ctx.Response.Body))
            {
                new JsonSerializer().Serialize(writer, Payload);
            }
        }
    }
}
