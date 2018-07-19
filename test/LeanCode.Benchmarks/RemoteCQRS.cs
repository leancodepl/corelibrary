using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;
using LeanCode.Components;
using LeanCode.CQRS;
using LeanCode.CQRS.Default;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.RemoteHttp.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace LeanCode.Benchmarks
{
    [CoreJob]
    [MarkdownExporterAttribute.Atlassian]
    [MemoryDiagnoser]
    public class RemoteCQRS
    {
        private static Assembly assembly = typeof(InProcCQRS__Commands).Assembly;
        private static TypesCatalog catalog = new TypesCatalog(assembly);

        private SampleAppContext appContext = new SampleAppContext();

        private IServiceProvider serviceProvider;
        private RemoteCQRSMiddleware<SampleAppContext> middleware;

        private byte[] empty;
        private byte[] multipleFields;

        [GlobalSetup]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            var module = new CQRSModule()
                .WithCustomPipelines<SampleAppContext>(catalog, b => b, b => b);
            builder.RegisterModule(module);

            builder.Populate(new ServiceDescriptor[0]);
            var container = builder.Build();

            middleware = new RemoteCQRSMiddleware<SampleAppContext>(
                catalog, _ => appContext,
                _ => Task.CompletedTask);
            serviceProvider = container.Resolve<IServiceProvider>();

            empty = GetContent(new SampleDTO());
            multipleFields = GetContent(new MultipleFieldsCommand
            {
                F1 = "test field",
                F2 = 123
            });
        }

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
            var str = JsonConvert.SerializeObject(obj);
            return System.Text.Encoding.UTF8.GetBytes(str);
        }

        private HttpContext PrepareCommand(bool multi)
        {
            var context = new DefaultHttpContext();
            context.RequestServices = serviceProvider;
            context.Request.Method = HttpMethods.Post;
            context.Request.ContentType = "application/json";
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
            var context = new DefaultHttpContext();
            context.RequestServices = serviceProvider;
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
