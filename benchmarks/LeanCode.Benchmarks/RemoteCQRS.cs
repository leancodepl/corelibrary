using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BenchmarkDotNet.Attributes;
using LeanCode.Components;
using LeanCode.CQRS;
using LeanCode.CQRS.Default;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.RemoteHttp.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Benchmarks
{
    [CoreJob]
    [MarkdownExporterAttribute.Atlassian]
    [MemoryDiagnoser]
    public class RemoteCQRS
    {
        private static readonly TypesCatalog Catalog = TypesCatalog.Of<RemoteCQRS>();

        private readonly SampleAppContext appContext = new SampleAppContext();
        private readonly InputToOutputMiddleware simpleMiddleware = new InputToOutputMiddleware();

        private IServiceProvider serviceProvider;
        private RemoteCQRSMiddleware<SampleAppContext> middleware;

        private byte[] empty;
        private byte[] multipleFields;

        [GlobalSetup]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            var module = new CQRSModule()
                .WithCustomPipelines<SampleAppContext>(Catalog, b => b, b => b);
            builder.RegisterModule(module);

            builder.Populate(new ServiceDescriptor[0]);
            var container = builder.Build();

            middleware = new RemoteCQRSMiddleware<SampleAppContext>(
                Catalog, _ => appContext,
                _ => Task.CompletedTask);
            serviceProvider = container.Resolve<IServiceProvider>();

            empty = GetContent(new SampleDTO());
            multipleFields = GetContent(new MultipleFieldsCommand
            {
                F1 = "test field",
                F2 = 123,
            });
        }

        [Benchmark(Baseline = true)]
        public Task SimpleMiddleware() =>
            simpleMiddleware.Invoke(PrepareQuery(true));

        [Benchmark]
        public Task EmptyQuery() =>
            middleware.Invoke(PrepareQuery(false));

        [Benchmark]
        public Task EmptyCommand() =>
            middleware.Invoke(PrepareCommand(false));

        [Benchmark]
        public Task MultipleFieldsQuery() =>
            middleware.Invoke(PrepareQuery(true));

        [Benchmark]
        public Task MultipleFieldsCommand() =>
            middleware.Invoke(PrepareCommand(true));

        private byte[] GetContent(object obj)
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj, obj.GetType());
        }

        private HttpContext PrepareCommand(bool multi)
        {
            var context = new DefaultHttpContext { RequestServices = serviceProvider };
            if (multi)
            {
                context.Request.Path = "/command/LeanCode.Benchmarks.MultipleFieldsCommand";
                context.Request.Headers.ContentLength = multipleFields.LongLength;
                context.Request.Body = new MemoryStream(multipleFields);
            }
            else
            {
                context.Request.Path = "/command/LeanCode.Benchmarks.PlainCommand";
                context.Request.Headers.ContentLength = empty.LongLength;
                context.Request.Body = new MemoryStream(empty);
            }

            return context;
        }

        private HttpContext PrepareQuery(bool multi)
        {
            var context = new DefaultHttpContext
            {
                RequestServices = serviceProvider,
            };
            context.Request.Method = HttpMethods.Post;
            context.Request.ContentType = "application/json";
            if (multi)
            {
                context.Request.Path = "/query/LeanCode.Benchmarks.MultipleFieldsQuery";
                context.Request.Headers.ContentLength = multipleFields.LongLength;
                context.Request.Body = new MemoryStream(multipleFields);
            }
            else
            {
                context.Request.Path = "/query/LeanCode.Benchmarks.PlainQuery";
                context.Request.Headers.ContentLength = empty.LongLength;
                context.Request.Body = new MemoryStream(empty);
            }

            return context;
        }
    }
}
