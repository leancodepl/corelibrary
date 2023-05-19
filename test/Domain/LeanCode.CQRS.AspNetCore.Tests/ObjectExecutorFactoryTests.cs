using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests;

public class ObjectExecutorFactoryTests
{
    private readonly MockServiceProvider mockServiceProvider = new();
    private readonly ObjectExecutorFactory executorFactory;

    public ObjectExecutorFactoryTests()
    {
        executorFactory = new ObjectExecutorFactory();
    }

    [Fact]
    public async Task Creates_executor_for_command_which_passes_payload_to_command_handler()
    {
        var commandHandler = mockServiceProvider.CommandHandler;

        var cmd = new Command();
        var ctx = MockHttpContext();

        var executorMethod = executorFactory.CreateExecutorFor(
            new CQRSObjectMetadata(
                CQRSObjectKind.Command,
                typeof(Command),
                typeof(CommandResult),
                typeof(CommandHandler)
            )
        );
        var result = await executorMethod(ctx, new CQRSRequestPayload(cmd));

        Assert.Same(cmd, commandHandler.ReceivedCommand);
        Assert.Same(ctx, commandHandler.ReceivedContext);
        var commandResult = Assert.IsType<CommandResult>(result);
        Assert.True(commandResult.WasSuccessful);
    }

    [Fact]
    public async Task Creates_query_handler_which_passes_payload_to_query_handler()
    {
        var queryHandler = mockServiceProvider.QueryHandler;

        var query = new Query();
        var ctx = MockHttpContext();

        var executorMethod = executorFactory.CreateExecutorFor(
            new CQRSObjectMetadata(CQRSObjectKind.Query, typeof(Query), typeof(QueryResult), typeof(QueryHandler))
        );
        var result = await executorMethod(ctx, new CQRSRequestPayload(query));

        Assert.Same(query, queryHandler.ReceivedQuery);
        Assert.Same(ctx, queryHandler.ReceivedContext);
        var queryResult = Assert.IsType<QueryResult>(result);
        Assert.Same(queryHandler.ReturnedResult, queryResult);
    }

    [Fact]
    public async Task Creates_operation_handler_which_passes_payload_to_operation_handler()
    {
        var operationHandler = mockServiceProvider.OperationHandler;

        var operation = new Operation();
        var ctx = MockHttpContext();

        var executorMethod = executorFactory.CreateExecutorFor(
            new CQRSObjectMetadata(
                CQRSObjectKind.Operation,
                typeof(Operation),
                typeof(OperationResult),
                typeof(OperationHandler)
            )
        );
        var result = await executorMethod(ctx, new CQRSRequestPayload(operation));

        Assert.Same(operation, operationHandler.ReceivedOperation);
        Assert.Same(ctx, operationHandler.ReceivedContext);
        var operationResult = Assert.IsType<OperationResult>(result);
        Assert.Same(operationHandler.ReturnedResult, operationResult);
    }

    private HttpContext MockHttpContext()
    {
        var context = Substitute.For<HttpContext>();
        context.RequestServices.Returns(mockServiceProvider);
        return context;
    }

    private class Command : ICommand { }

    private class Query : IQuery<QueryResult> { }

    private class QueryResult { }

    private class Operation : IOperation<OperationResult> { }

    private class OperationResult { }

    private class CommandHandler : ICommandHandler<Command>
    {
        public HttpContext? ReceivedContext { get; private set; }
        public Command? ReceivedCommand { get; private set; }

        public Task ExecuteAsync(HttpContext context, Command command)
        {
            ReceivedContext = context;
            ReceivedCommand = command;

            return Task.CompletedTask;
        }
    }

    private class QueryHandler : IQueryHandler<Query, QueryResult>
    {
        public HttpContext? ReceivedContext { get; private set; }
        public Query? ReceivedQuery { get; private set; }
        public QueryResult ReturnedResult { get; } = new QueryResult();

        public Task<QueryResult> ExecuteAsync(HttpContext context, Query query)
        {
            ReceivedContext = context;
            ReceivedQuery = query;

            return Task.FromResult(ReturnedResult);
        }
    }

    private class OperationHandler : IOperationHandler<Operation, OperationResult>
    {
        public HttpContext? ReceivedContext { get; private set; }
        public Operation? ReceivedOperation { get; private set; }
        public OperationResult ReturnedResult { get; } = new OperationResult();

        public Task<OperationResult> ExecuteAsync(HttpContext context, Operation operation)
        {
            ReceivedContext = context;
            ReceivedOperation = operation;

            return Task.FromResult(ReturnedResult);
        }
    }

    private class MockServiceProvider : IServiceProvider
    {
        public CommandHandler CommandHandler { get; } = new();
        public QueryHandler QueryHandler { get; } = new();
        public OperationHandler OperationHandler { get; } = new();

        public object? GetService(Type type)
        {
            if (type == typeof(ICommandHandler<Command>))
            {
                return CommandHandler;
            }
            else if (type == typeof(IQueryHandler<Query, QueryResult>))
            {
                return QueryHandler;
            }
            else if (type == typeof(IOperationHandler<Operation, OperationResult>))
            {
                return OperationHandler;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
