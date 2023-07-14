using FluentAssertions;
using LeanCode.Contracts;
using LeanCode.CQRS.Annotations;
using LeanCode.CQRS.AspNetCore.Tests.CQRSEndpointsDataSourceTestsContracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests
{
    public sealed class CQRSEndpointsDataSourceTests : IDisposable, IAsyncLifetime
    {
        private readonly IHost host;
        private readonly TestServer server;

        public CQRSEndpointsDataSourceTests()
        {
            host = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost
                        .UseTestServer()
                        .ConfigureServices(services => services.AddRouting())
                        .Configure(app =>
                        {
                            app.UseRouting();
                            app.UseEndpoints(e =>
                            {
                                e.DataSources.Add(PrepareEndpointsSource());
                            });
                        });
                })
                .Build();
            server = host.GetTestServer();
        }

        [Fact]
        public async Task Correct_routes_are_registered_for_objects()
        {
            await VerifyValidPath<Command>(
                "/cqrs/command/LeanCode.CQRS.AspNetCore.Tests.CQRSEndpointsDataSourceTestsContracts.Command"
            );

            await VerifyValidPath<AliasedCommand>(
                "/cqrs/command/LeanCode.CQRS.AspNetCore.Tests.CQRSEndpointsDataSourceTestsContracts.AliasedCommand"
            );
            await VerifyValidPath<AliasedCommand>(
                "/cqrs/command/LeanCode.CQRS.AlternativeNamespace.AlternativeCommandName"
            );
            await VerifyValidPath<AliasedCommand>(
                "/cqrs/command/LeanCode.CQRS.AlternativeNamespace.AlternativeCommandName2"
            );

            await VerifyValidPath<Query>(
                "/cqrs/query/LeanCode.CQRS.AspNetCore.Tests.CQRSEndpointsDataSourceTestsContracts.Query"
            );

            await VerifyValidPath<AliasedQuery>(
                "/cqrs/query/LeanCode.CQRS.AspNetCore.Tests.CQRSEndpointsDataSourceTestsContracts.AliasedQuery"
            );
            await VerifyValidPath<AliasedQuery>("/cqrs/query/LeanCode.CQRS.AlternativeNamespace.AlternativeQueryName");

            await VerifyValidPath<Operation>(
                "/cqrs/operation/LeanCode.CQRS.AspNetCore.Tests.CQRSEndpointsDataSourceTestsContracts.Operation"
            );

            await VerifyValidPath<AliasedOperation>(
                "/cqrs/operation/LeanCode.CQRS.AspNetCore.Tests.CQRSEndpointsDataSourceTestsContracts.AliasedOperation"
            );
            await VerifyValidPath<AliasedOperation>(
                "/cqrs/operation/LeanCode.CQRS.AlternativeNamespace.AlternativeOperationName"
            );
        }

        [Fact]
        public async Task Wrong_calls_return_error_code()
        {
            // wrong method
            await VerifyWrongPath(
                "/cqrs/command/LeanCode.CQRS.AspNetCore.Tests.CQRSEndpointsDataSourceTestsContracts.Command",
                "GET",
                StatusCodes.Status405MethodNotAllowed
            );
            await VerifyWrongPath(
                "/cqrs/command/LeanCode.CQRS.AspNetCore.Tests.CQRSEndpointsDataSourceTestsContracts.Command",
                "GET",
                StatusCodes.Status405MethodNotAllowed
            );

            // wrong kind
            await VerifyWrongPath(
                "/cqrs/command/LeanCode.CQRS.AspNetCore.Tests.CQRSEndpointsDataSourceTestsContracts.Query"
            );
            await VerifyWrongPath(
                "/cqrs/query/LeanCode.CQRS.AspNetCore.Tests.CQRSEndpointsDataSourceTestsContracts.Operation"
            );

            // non-existing command

            await VerifyWrongPath("/cqrs/command/LeanCode.CQRS.NonExisting");
        }

        private async Task VerifyValidPath<TObject>(string path)
        {
            var ctx = await server.SendAsync(ctx =>
            {
                ctx.Request.Path = path;
                ctx.Request.Method = "POST";
            });

            ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
            var cqrsMetadata = ctx.GetCQRSEndpoint().ObjectMetadata;
            cqrsMetadata.ObjectType.Should().Be(typeof(TObject));
        }

        private async Task VerifyWrongPath(string path, string method = "POST", int statusCode = 404)
        {
            var ctx = await server.SendAsync(ctx =>
            {
                ctx.Request.Path = path;
                ctx.Request.Method = method;
            });

            ctx.Response.StatusCode.Should().Be(statusCode);
        }

        private static CQRSEndpointsDataSource PrepareEndpointsSource()
        {
            var dataSource = new CQRSEndpointsDataSource("/cqrs", new MockExecutorFactory());

            RequestDelegate pipeline = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status200OK;
                return Task.CompletedTask;
            };

            dataSource.AddEndpointsFor(
                new CQRSObjectMetadata[]
                {
                    new(CQRSObjectKind.Command, typeof(Command), typeof(CommandResult), typeof(IgnoreHandler)),
                    new(CQRSObjectKind.Command, typeof(AliasedCommand), typeof(CommandResult), typeof(IgnoreHandler)),
                    new(CQRSObjectKind.Query, typeof(Query), typeof(Result), typeof(IgnoreHandler)),
                    new(CQRSObjectKind.Query, typeof(AliasedQuery), typeof(Result), typeof(IgnoreHandler)),
                    new(CQRSObjectKind.Operation, typeof(Operation), typeof(Result), typeof(IgnoreHandler)),
                    new(CQRSObjectKind.Operation, typeof(AliasedOperation), typeof(Result), typeof(IgnoreHandler)),
                },
                pipeline,
                pipeline,
                pipeline
            );
            return dataSource;
        }

        public void Dispose()
        {
            server.Dispose();
            host.Dispose();
        }

        public Task InitializeAsync() => host.StartAsync();

        public Task DisposeAsync() => host.StopAsync();

        private sealed class MockExecutorFactory : IObjectExecutorFactory
        {
            public ObjectExecutor CreateExecutorFor(CQRSObjectMetadata @object) =>
                (httpContext, payload) => Task.FromResult(null as object);
        }

        private sealed class IgnoreHandler { }
    }
}

namespace LeanCode.CQRS.AspNetCore.Tests.CQRSEndpointsDataSourceTestsContracts
{
    public class Result { }

    public class Query : IQuery<Result> { }

    [PathAlias("LeanCode.CQRS.AlternativeNamespace.AlternativeQueryName")]
    public class AliasedQuery : IQuery<Result> { }

    public class Command : ICommand { }

    [PathAlias("LeanCode.CQRS.AlternativeNamespace.AlternativeCommandName")]
    [PathAlias("LeanCode.CQRS.AlternativeNamespace.AlternativeCommandName2")]
    public class AliasedCommand : ICommand { }

    public class Operation : IOperation<Result> { }

    [PathAlias("LeanCode.CQRS.AlternativeNamespace.AlternativeOperationName")]
    public class AliasedOperation : IOperation<Result> { }
}
