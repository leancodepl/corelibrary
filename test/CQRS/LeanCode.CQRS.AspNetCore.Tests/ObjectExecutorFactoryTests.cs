using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests;

[SuppressMessage(category: "?", "CA1034", Justification = "Nesting public types for better tests separation")]
public class ObjectExecutorFactoryTests
{
    private readonly ObjectExecutorFactory executorFactory = new ObjectExecutorFactory();
    private readonly IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();

    private readonly CommandHandler commandHandler = Substitute.For<CommandHandler>();
    private readonly QueryHandler queryHandler = Substitute.For<QueryHandler>();
    private readonly OperationHandler operationHandler = Substitute.For<OperationHandler>();

    public ObjectExecutorFactoryTests()
    {
        serviceProvider.GetService(typeof(CommandHandler)).Returns(commandHandler);
        serviceProvider.GetService(typeof(QueryHandler)).Returns(queryHandler);
        serviceProvider.GetService(typeof(OperationHandler)).Returns(operationHandler);
    }

    [Fact]
    public async Task Creates_executor_for_command_which_passes_payload_to_command_handler()
    {
        var cmd = new Command();
        var ctx = MockHttpContext();

        var executorMethod = executorFactory.CreateExecutorFor(
            CQRSObjectKind.Command,
            typeof(Command),
            typeof(CommandHandler)
        );
        var result = await executorMethod(ctx, new CQRSRequestPayload(cmd));

        await commandHandler.Received().ExecuteAsync(ctx, cmd);
        result.Should().BeOfType<CommandResult>().Which.WasSuccessful.Should().BeTrue();
    }

    [Fact]
    public async Task Resolves_the_exact_command_handler_from_DI()
    {
        var cmd = new Command();
        var ctx = MockHttpContext();

        serviceProvider
            .GetService(typeof(ICommandHandler<Command>))
            .Returns(Substitute.For<ICommandHandler<Command>>());

        var executorMethod = executorFactory.CreateExecutorFor(
            CQRSObjectKind.Command,
            typeof(Command),
            typeof(CommandHandler)
        );
        await executorMethod(ctx, new CQRSRequestPayload(cmd));

        await commandHandler.Received().ExecuteAsync(ctx, cmd);
    }

    [Fact]
    public async Task Creates_query_handler_which_passes_payload_to_query_handler()
    {
        var query = new Query();
        var ctx = MockHttpContext();
        var returnedResult = new QueryResult();
        queryHandler.ExecuteAsync(ctx, query).Returns(returnedResult);

        var executorMethod = executorFactory.CreateExecutorFor(
            CQRSObjectKind.Query,
            typeof(Query),
            typeof(QueryHandler)
        );

        var result = await executorMethod(ctx, new CQRSRequestPayload(query));

        await queryHandler.Received().ExecuteAsync(ctx, query);
        result.Should().BeOfType<QueryResult>().Which.Should().BeSameAs(returnedResult);
    }

    [Fact]
    public async Task Resolves_the_exact_query_handler_from_DI()
    {
        var query = new Query();
        var ctx = MockHttpContext();

        serviceProvider
            .GetService(typeof(IQueryHandler<Query, QueryResult>))
            .Returns(Substitute.For<IQueryHandler<Query, QueryResult>>());

        var executorMethod = executorFactory.CreateExecutorFor(
            CQRSObjectKind.Query,
            typeof(Query),
            typeof(QueryHandler)
        );
        await executorMethod(ctx, new CQRSRequestPayload(query));

        await queryHandler.Received().ExecuteAsync(ctx, Arg.Any<Query>());
    }

    [Fact]
    public async Task Creates_operation_handler_which_passes_payload_to_operation_handler()
    {
        var operation = new Operation();
        var ctx = MockHttpContext();
        var returnedResult = new OperationResult();
        operationHandler.ExecuteAsync(ctx, operation).Returns(returnedResult);

        var executorMethod = executorFactory.CreateExecutorFor(
            CQRSObjectKind.Operation,
            typeof(Operation),
            typeof(OperationHandler)
        );
        var result = await executorMethod(ctx, new CQRSRequestPayload(operation));

        await operationHandler.Received().ExecuteAsync(ctx, operation);
        result.Should().BeOfType<OperationResult>().Which.Should().BeSameAs(returnedResult);
    }

    [Fact]
    public async Task Resolves_the_exact_operation_handler_from_DI()
    {
        var operation = new Operation();
        var ctx = MockHttpContext();

        serviceProvider
            .GetService(typeof(IOperationHandler<Operation, OperationResult>))
            .Returns(Substitute.For<IOperationHandler<Operation, OperationResult>>());

        var executorMethod = executorFactory.CreateExecutorFor(
            CQRSObjectKind.Operation,
            typeof(Operation),
            typeof(OperationHandler)
        );
        await executorMethod(ctx, new CQRSRequestPayload(operation));

        await operationHandler.Received().ExecuteAsync(ctx, Arg.Any<Operation>());
    }

    [Fact]
    public async Task Throws_command_handler_not_found_if_cannot_get_it_from_DI()
    {
        var cmd = new Command();
        var ctx = MockHttpContext();

        serviceProvider.GetService(typeof(CommandHandler)).Returns(null);
        var executorMethod = executorFactory.CreateExecutorFor(
            CQRSObjectKind.Command,
            typeof(Command),
            typeof(CommandHandler)
        );

        var act = () => executorMethod(ctx, new CQRSRequestPayload(cmd));
        await act.Should().ThrowAsync<CommandHandlerNotFoundException>();
    }

    [Fact]
    public async Task Throws_query_handler_not_found_if_cannot_get_it_from_DI()
    {
        var query = new Query();
        var ctx = MockHttpContext();

        serviceProvider.GetService(typeof(QueryHandler)).Returns(null);
        var executorMethod = executorFactory.CreateExecutorFor(
            CQRSObjectKind.Query,
            typeof(Query),
            typeof(QueryHandler)
        );

        var act = () => executorMethod(ctx, new CQRSRequestPayload(query));
        await act.Should().ThrowAsync<QueryHandlerNotFoundException>();
    }

    [Fact]
    public async Task Throws_operation_handler_not_found_if_cannot_get_it_from_DI()
    {
        var operation = new Operation();
        var ctx = MockHttpContext();

        serviceProvider.GetService(typeof(OperationHandler)).Returns(null);
        var executorMethod = executorFactory.CreateExecutorFor(
            CQRSObjectKind.Operation,
            typeof(Operation),
            typeof(OperationHandler)
        );

        var act = () => executorMethod(ctx, new CQRSRequestPayload(operation));
        await act.Should().ThrowAsync<OperationHandlerNotFoundException>();
    }

    private HttpContext MockHttpContext()
    {
        var context = Substitute.For<HttpContext>();
        context.RequestServices.Returns(serviceProvider);
        return context;
    }

    public class Command : ICommand { }

    public class CommandHandler : ICommandHandler<Command>
    {
        public Task ExecuteAsync(HttpContext context, Command command) => Task.CompletedTask;
    }

    public class QueryResult { }

    public class Query : IQuery<QueryResult> { }

    public class QueryHandler : IQueryHandler<Query, QueryResult>
    {
        public virtual Task<QueryResult> ExecuteAsync(HttpContext context, Query query) =>
            Task.FromResult(new QueryResult());
    }

    public class Operation : IOperation<OperationResult> { }

    public class OperationResult { }

    public class OperationHandler : IOperationHandler<Operation, OperationResult>
    {
        public virtual Task<OperationResult> ExecuteAsync(HttpContext context, Operation operation) =>
            Task.FromResult(new OperationResult());
    }
}
