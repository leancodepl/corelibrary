using FluentAssertions;
using LeanCode.Components;
using LeanCode.Contracts;
using LeanCode.CQRS.AspNetCore.Registration;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Registration;

public class CQRSApiDescriptionProviderTests
{
    private const string BasePath = "cqrs";

    private static readonly Dictionary<string, string?> ExpectedRouteValues = new() { ["controller"] = "CQRS" };
    private static readonly List<ApiRequestFormat> ExpectedRequestFormat =
    [
        new ApiRequestFormat { MediaType = "application/json" }
    ];

    [Fact]
    public void ApiDescriptionProvider_is_registered_correctly()
    {
        var sc = new ServiceCollection();
        sc.AddTransient<EndpointDataSource, DummyEndpointDataSource>();
        sc.AddCQRSApiExplorer();
        var provider = sc.BuildServiceProvider();

        var providers = provider.GetService<IEnumerable<IApiDescriptionProvider>>();
        providers.Should().ContainSingle().Which.Should().BeOfType<CQRSApiDescriptionProvider>();
    }

    [Fact]
    public void Describes_base_query_parameters()
    {
        var allDescriptors = ListApisFor<Query>();

        var query = allDescriptors.Should().ContainSingle().Which;

        query.HttpMethod.Should().Be("POST");
        query.RelativePath.Should().Be($"{BasePath}/query/{typeof(Query).FullName}");
        query.ActionDescriptor.DisplayName.Should().Be($"Query {typeof(Query).FullName}");
        query.ActionDescriptor.RouteValues.Should().BeEquivalentTo(ExpectedRouteValues);
        query.SupportedRequestFormats.Should().BeEquivalentTo(ExpectedRequestFormat);
    }

    [Fact]
    public void Query_has_a_single_parameter_that_describes_body()
    {
        var allDescriptors = ListApisFor<Query>();

        var query = allDescriptors.Should().ContainSingle().Which;
        query.ParameterDescriptions.Should().ContainSingle().Which.Should().BeEquivalentTo(RequestOf<Query>());
    }

    [Fact]
    public void Query_defines_all_responses()
    {
        var allDescriptors = ListApisFor<Query>();

        var query = allDescriptors.Should().ContainSingle().Which;
        query
            .SupportedResponseTypes
            .Should()
            .BeEquivalentTo(
                [ ResponseOf<QueryResultDTO>(200), ResponseOfVoid(400), ResponseOfVoid(401), ResponseOfVoid(403), ]
            );
    }

    [Fact]
    public void Describes_base_command_parameters()
    {
        var allDescriptors = ListApisFor<Command>();

        var query = allDescriptors.Should().ContainSingle().Which;

        query.HttpMethod.Should().Be("POST");
        query.RelativePath.Should().Be($"{BasePath}/command/{typeof(Command).FullName}");
        query.ActionDescriptor.DisplayName.Should().Be($"Command {typeof(Command).FullName}");
        query.ActionDescriptor.RouteValues.Should().BeEquivalentTo(ExpectedRouteValues);
        query.SupportedRequestFormats.Should().BeEquivalentTo(ExpectedRequestFormat);
    }

    [Fact]
    public void Command_has_a_single_parameter_that_describes_body()
    {
        var allDescriptors = ListApisFor<Command>();

        var query = allDescriptors.Should().ContainSingle().Which;
        query.ParameterDescriptions.Should().ContainSingle().Which.Should().BeEquivalentTo(RequestOf<Command>());
    }

    [Fact]
    public void Command_defines_all_responses()
    {
        var allDescriptors = ListApisFor<Command>();

        var query = allDescriptors.Should().ContainSingle().Which;
        query
            .SupportedResponseTypes
            .Should()
            .BeEquivalentTo(
                [
                    ResponseOf<CommandResult>(200),
                    ResponseOf<CommandResult>(422),
                    ResponseOfVoid(400),
                    ResponseOfVoid(401),
                    ResponseOfVoid(403),
                ]
            );
    }

    [Fact]
    public void Describes_base_operation_parameters()
    {
        var allDescriptors = ListApisFor<Operation>();

        var query = allDescriptors.Should().ContainSingle().Which;

        query.HttpMethod.Should().Be("POST");
        query.RelativePath.Should().Be($"{BasePath}/operation/{typeof(Operation).FullName}");
        query.ActionDescriptor.DisplayName.Should().Be($"Operation {typeof(Operation).FullName}");
        query.ActionDescriptor.RouteValues.Should().BeEquivalentTo(ExpectedRouteValues);
        query.SupportedRequestFormats.Should().BeEquivalentTo(ExpectedRequestFormat);
    }

    [Fact]
    public void Operation_has_a_single_parameter_that_describes_body()
    {
        var allDescriptors = ListApisFor<Operation>();

        var query = allDescriptors.Should().ContainSingle().Which;
        query.ParameterDescriptions.Should().ContainSingle().Which.Should().BeEquivalentTo(RequestOf<Operation>());
    }

    [Fact]
    public void Operation_defines_all_responses()
    {
        var allDescriptors = ListApisFor<Operation>();

        var query = allDescriptors.Should().ContainSingle().Which;
        query
            .SupportedResponseTypes
            .Should()
            .BeEquivalentTo(
                [ ResponseOf<OperationResultDTO>(200), ResponseOfVoid(400), ResponseOfVoid(401), ResponseOfVoid(403), ]
            );
    }

    private static object RequestOf<T>()
    {
        return new
        {
            IsRequired = true,
            Source = BindingSource.Body,
            Type = typeof(T),
            ModelMetadata = new { Identity = new { ModelType = typeof(T) } },
        };
    }

    private static object ResponseOf<T>(int statusCode)
    {
        return new
        {
            ApiResponseFormats = new[] { new { MediaType = "application/json" } },
            StatusCode = statusCode,
            Type = typeof(T),
            ModelMetadata = new { Identity = new { ModelType = typeof(T) } },
        };
    }

    private static object ResponseOfVoid(int statusCode)
    {
        return new
        {
            ApiResponseFormats = new[] { new { MediaType = "application/json" } },
            StatusCode = statusCode,
            Type = typeof(void),
            ModelMetadata = new { Identity = new { ModelType = typeof(void) } },
        };
    }

    private static List<ApiDescription> ListApisFor<T>()
    {
        return ListApis(typeof(T));
    }

    private static List<ApiDescription> ListApis(Type forObject)
    {
        var dataSource = CreateDataSource(forObject);
        var context = new ApiDescriptionProviderContext([ ]);
        new CQRSApiDescriptionProvider(dataSource).OnProvidersExecuting(context);
        return context.Results.ToList();
    }

    private static CQRSEndpointsDataSource CreateDataSource(Type forObject)
    {
        var selfCatalog = TypesCatalog.Of<CQRSApiDescriptionProviderTests>();
        var regSource = new CQRSObjectsRegistrationSource(new ServiceCollection());
        regSource.AddCQRSObjects(selfCatalog, selfCatalog);

        var dataSource = new CQRSEndpointsDataSource("/" + BasePath, new ObjectExecutorFactory());
        dataSource.AddEndpointsFor(
            regSource.Objects.Where(o => o.ObjectType == forObject),
            _ => Task.CompletedTask,
            _ => Task.CompletedTask,
            _ => Task.CompletedTask
        );
        return dataSource;
    }
}

public record QueryResultDTO();

public record Query() : IQuery<QueryResultDTO>;

public class QueryQH : IQueryHandler<Query, QueryResultDTO>
{
    public Task<QueryResultDTO> ExecuteAsync(HttpContext context, Query query) => throw new NotImplementedException();
}

public record Command() : ICommand;

public class CommandCH : ICommandHandler<Command>
{
    public Task ExecuteAsync(HttpContext context, Command command) => throw new NotImplementedException();
}

public record OperationResultDTO();

public record Operation() : IOperation<OperationResultDTO>;

public class OperationOH : IOperationHandler<Operation, OperationResultDTO>
{
    public Task<OperationResultDTO> ExecuteAsync(HttpContext context, Operation operation) =>
        throw new NotImplementedException();
}

internal class DummyEndpointDataSource : EndpointDataSource
{
    public override IReadOnlyList<Endpoint> Endpoints => throw new NotImplementedException();

    public override IChangeToken GetChangeToken() => throw new NotImplementedException();
}
