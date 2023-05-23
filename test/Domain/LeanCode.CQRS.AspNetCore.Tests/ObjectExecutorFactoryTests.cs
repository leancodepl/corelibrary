using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests;

public class ObjectExecutorFactoryTests
{
    private readonly ObjectExecutorFactory executorFactory;
    private readonly IServiceProvider serviceProvider;
    private readonly ICommandHandler<Command> commandHandler;
    private readonly IQueryHandler<Query, QueryResult> queryHandler;
    private readonly IOperationHandler<Operation, OperationResult> operationHandler;

    private readonly CQRSObjectMetadata queryMetadata =
        new(CQRSObjectKind.Query, typeof(Query), typeof(QueryResult), typeof(IgnoreType));

    private readonly CQRSObjectMetadata commandMetadata =
        new(CQRSObjectKind.Command, typeof(Command), typeof(CommandResult), typeof(IgnoreType));

    private readonly CQRSObjectMetadata operationMetadata =
        new(CQRSObjectKind.Operation, typeof(Operation), typeof(OperationResult), typeof(IgnoreType));

    public ObjectExecutorFactoryTests()
    {
        executorFactory = new ObjectExecutorFactory();
        serviceProvider = Substitute.For<IServiceProvider>();
        commandHandler = Substitute.For<ICommandHandler<Command>>();
        queryHandler = Substitute.For<IQueryHandler<Query, QueryResult>>();
        operationHandler = Substitute.For<IOperationHandler<Operation, OperationResult>>();

        serviceProvider.GetService(typeof(ICommandHandler<Command>)).Returns(commandHandler);
        serviceProvider.GetService(typeof(IQueryHandler<Query, QueryResult>)).Returns(queryHandler);
        serviceProvider.GetService(typeof(IOperationHandler<Operation, OperationResult>)).Returns(operationHandler);
    }

    [Fact]
    public async Task Creates_executor_for_command_which_passes_payload_to_command_handler()
    {
        var cmd = new Command();
        var ctx = MockHttpContext();

        var executorMethod = executorFactory.CreateExecutorFor(commandMetadata);
        var result = await executorMethod(ctx, new CQRSRequestPayload(cmd));

        await commandHandler.Received().ExecuteAsync(ctx, cmd);
        var commandResult = Assert.IsType<CommandResult>(result);
        Assert.True(commandResult.WasSuccessful);
    }

    [Fact]
    public async Task Creates_query_handler_which_passes_payload_to_query_handler()
    {
        var query = new Query();
        var ctx = MockHttpContext();
        var returnedResult = new QueryResult();
        queryHandler.ExecuteAsync(ctx, query).Returns(returnedResult);

        var executorMethod = executorFactory.CreateExecutorFor(queryMetadata);

        var result = await executorMethod(ctx, new CQRSRequestPayload(query));

        await queryHandler.Received().ExecuteAsync(ctx, query);
        var queryResult = Assert.IsType<QueryResult>(result);
        Assert.Same(returnedResult, queryResult);
    }

    [Fact]
    public async Task Creates_operation_handler_which_passes_payload_to_operation_handler()
    {
        var operation = new Operation();
        var ctx = MockHttpContext();
        var returnedResult = new OperationResult();
        operationHandler.ExecuteAsync(ctx, operation).Returns(returnedResult);

        var executorMethod = executorFactory.CreateExecutorFor(operationMetadata);
        var result = await executorMethod(ctx, new CQRSRequestPayload(operation));

        await operationHandler.Received().ExecuteAsync(ctx, operation);
        var operationResult = Assert.IsType<OperationResult>(result);
        Assert.Same(returnedResult, operationResult);
    }

    [Fact]
    public async Task Throws_command_handler_not_found_if_cannot_get_it_from_DI()
    {
        var cmd = new Command();
        var ctx = MockHttpContext();

        serviceProvider.GetService(typeof(ICommandHandler<Command>)).Returns(null);
        var executorMethod = executorFactory.CreateExecutorFor(commandMetadata);

        await Assert.ThrowsAsync<CommandHandlerNotFoundException>(
            () => executorMethod(ctx, new CQRSRequestPayload(cmd))
        );
    }

    [Fact]
    public async Task Throws_query_handler_not_found_if_cannot_get_it_from_DI()
    {
        var query = new Query();
        var ctx = MockHttpContext();

        serviceProvider.GetService(typeof(IQueryHandler<Query, QueryResult>)).Returns(null);
        var executorMethod = executorFactory.CreateExecutorFor(queryMetadata);

        await Assert.ThrowsAsync<QueryHandlerNotFoundException>(
            () => executorMethod(ctx, new CQRSRequestPayload(queryHandler))
        );
    }

    [Fact]
    public async Task Throws_operation_handler_not_found_if_cannot_get_it_from_DI()
    {
        var operation = new Operation();
        var ctx = MockHttpContext();

        serviceProvider.GetService(typeof(IOperationHandler<Operation, OperationResult>)).Returns(null);
        var executorMethod = executorFactory.CreateExecutorFor(operationMetadata);

        await Assert.ThrowsAsync<OperationHandlerNotFoundException>(
            () => executorMethod(ctx, new CQRSRequestPayload(operation))
        );
    }

    private HttpContext MockHttpContext()
    {
        var context = Substitute.For<HttpContext>();
        context.RequestServices.Returns(serviceProvider);
        return context;
    }

    public class Command : ICommand { }

    public class Query : IQuery<QueryResult> { }

    public class QueryResult { }

    public class Operation : IOperation<OperationResult> { }

    public class OperationResult { }

    public class IgnoreType { }
}
