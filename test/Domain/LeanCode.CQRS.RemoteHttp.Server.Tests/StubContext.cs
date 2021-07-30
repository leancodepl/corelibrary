using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    internal class StubContext : HttpContext
    {
        public override HttpRequest Request { get; }
        public override HttpResponse Response { get; }
        public override IServiceProvider RequestServices { get; set; }
        public override CancellationToken RequestAborted { get; set; }

        public StubContext(string method, string path, string content)
        {
            Request = new StubRequest(this, method, path, content);
            Response = new StubResponse(this);

            RequestServices = null!;
            User = new ClaimsPrincipal();
        }

        public override IFeatureCollection Features => throw new NotImplementedException();
        public override ConnectionInfo Connection => throw new NotImplementedException();
        public override WebSocketManager WebSockets => throw new NotImplementedException();
        public override ClaimsPrincipal User { get; set; }
#pragma warning disable CS8609, CS8610 // It's absurd.
        public override IDictionary<object, object> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
#pragma warning restore CS8609, CS8610
        public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override void Abort() => throw new NotImplementedException();
    }

    internal class StubRequest : HttpRequest
    {
        public override HttpContext HttpContext { get; }
        public override string Method { get; set; }
        public override string Scheme { get; set; } = "http";
        public override bool IsHttps { get; set; }
        public override HostString Host { get; set; }
        public override PathString PathBase { get; set; }
        public override PathString Path { get; set; }
        public override QueryString QueryString { get; set; }
        public override IQueryCollection Query { get; set; }
        public override string Protocol { get; set; }
        public override string ContentType { get; set; }
        public override long? ContentLength { get; set; }
        public override Stream Body { get; set; }

        public StubRequest(HttpContext ctx, string method, string path, string content)
        {
            HttpContext = ctx;
            Method = method;
            Path = new PathString(path);
            ContentType = "application/json";

            var bytes = Encoding.UTF8.GetBytes(content);
            Body = new MemoryStream(bytes);
            ContentLength = bytes.Length;

            Query = new QueryCollection();
            Protocol = "";
        }

        public override IHeaderDictionary Headers => throw new NotImplementedException();
        public override IRequestCookieCollection Cookies { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override bool HasFormContentType => throw new NotImplementedException();
        public override IFormCollection Form { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    internal class StubResponse : HttpResponse
    {
        public override HttpContext HttpContext { get; }
        public override int StatusCode { get; set; }
        public override Stream Body { get; set; }
        public override long? ContentLength { get; set; }
        public override string ContentType { get; set; }

        public StubResponse(HttpContext ctx)
        {
            HttpContext = ctx;
            Body = new MemoryStream();
            ContentType = "text/plain";
        }

        public override IHeaderDictionary Headers => throw new NotImplementedException();
        public override IResponseCookies Cookies => throw new NotImplementedException();
        public override bool HasStarted => throw new NotImplementedException();
        public override void OnCompleted(Func<object, Task> callback, object state) => throw new NotImplementedException();
        public override void OnStarting(Func<object, Task> callback, object state) => throw new NotImplementedException();
        public override void Redirect(string location, bool permanent) => throw new NotImplementedException();
    }
}
