using System.Diagnostics.CodeAnalysis;
using LeanCode.Components;
using LeanCode.Contracts;
using LeanCode.CQRS.AspNetCore.Registration;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests;

[SuppressMessage(category: "?", "CA1034", Justification = "Nesting public types for better tests separation")]
public class CQRSObjectsRegistrationSourceTests
{
    private readonly List<CQRSObjectMetadata> cqrsObjects;

    public CQRSObjectsRegistrationSourceTests()
    {
        cqrsObjects = GetCQRSObjects();
    }

    [Fact]
    public void Finds_valid_objects()
    {
        AssertRegistered<Query1, QueryResult1, Query1Handler>();
        AssertRegistered<Query2, QueryResult2, Query2Handler>();
        AssertRegistered<Query3, QueryResult1, Query3Query4Handler>();
        AssertRegistered<Query4, QueryResult2, Query3Query4Handler>();
        AssertRegistered<Command1, Command1Handler>();

        AssertNotRegistered<QueryWithTwoResults>();
        AssertNotRegistered<QueryWithTwoHandlers>();
        AssertNotRegistered<QueryCommand>();
    }

    private void AssertRegistered<TQuery, TResult, THandler>()
        where TQuery : IQuery<TResult>
        where THandler : IQueryHandler<TQuery, TResult>
    {
        var cqrsObject = Assert.Single(cqrsObjects, o => o.ObjectType == typeof(TQuery));

        Assert.Equal(CQRSObjectKind.Query, cqrsObject.ObjectKind);
        Assert.Equal(typeof(TResult), cqrsObject.ResultType);
        Assert.Equal(typeof(THandler), cqrsObject.HandlerType);
    }

    private void AssertRegistered<TCommand, THandler>()
        where TCommand : ICommand
        where THandler : ICommandHandler<TCommand>
    {
        var cqrsObject = Assert.Single(cqrsObjects, o => o.ObjectType == typeof(TCommand));

        Assert.Equal(CQRSObjectKind.Command, cqrsObject.ObjectKind);
        Assert.Equal(typeof(CommandResult), cqrsObject.ResultType);
        Assert.Equal(typeof(THandler), cqrsObject.HandlerType);
    }

    private void AssertNotRegistered<T>()
    {
        Assert.DoesNotContain(cqrsObjects, o => o.ObjectType == typeof(T));
    }

    private static List<CQRSObjectMetadata> GetCQRSObjects()
    {
        return new CQRSObjectsRegistrationSource(
            TypesCatalog.Of<CQRSObjectsRegistrationSourceTests>(),
            TypesCatalog.Of<CQRSObjectsRegistrationSourceTests>()
        ).Objects.ToList();
        // This will find more types, filtering for better readability
    }

    public class Query1 : IQuery<QueryResult1> { }

    public class QueryResult1 { }

    public class Query2 : IQuery<QueryResult2> { }

    public class QueryResult2 { }

    public class Query3 : IQuery<QueryResult1> { }

    public class Query4 : IQuery<QueryResult2> { }

    public class QueryWithTwoResults : IQuery<QueryResult1>, IQuery<QueryResult2> { }

    public class QueryWithTwoHandlers : IQuery<QueryResult1> { }

    public class QueryCommand : IQuery<QueryResult1>, ICommand { }

    public class Command1 : ICommand { }

    public class Query1Handler : IQueryHandler<Query1, QueryResult1>
    {
        public Task<QueryResult1> ExecuteAsync(HttpContext context, Query1 query) =>
            throw new NotImplementedException();
    }

    public class Query2Handler : IQueryHandler<Query2, QueryResult2>
    {
        public Task<QueryResult2> ExecuteAsync(HttpContext context, Query2 query) =>
            throw new NotImplementedException();
    }

    public class DuplicatingHandler1 : IQueryHandler<QueryWithTwoHandlers, QueryResult1>
    {
        public Task<QueryResult1> ExecuteAsync(HttpContext context, QueryWithTwoHandlers query) =>
            throw new NotImplementedException();
    }

    public class DuplicatingHandler2 : IQueryHandler<QueryWithTwoHandlers, QueryResult1>
    {
        public Task<QueryResult1> ExecuteAsync(HttpContext context, QueryWithTwoHandlers query) =>
            throw new NotImplementedException();
    }

    public class Command1Handler : ICommandHandler<Command1>
    {
        public Task ExecuteAsync(HttpContext context, Command1 command) => throw new NotImplementedException();
    }

    public class QueryCommandHandler : ICommandHandler<QueryCommand>, IQueryHandler<QueryCommand, QueryResult1>
    {
        Task ICommandHandler<QueryCommand>.ExecuteAsync(HttpContext context, QueryCommand command) =>
            ExecuteAsync(context, command);

        public Task<QueryResult1> ExecuteAsync(HttpContext context, QueryCommand query) =>
            throw new NotImplementedException();
    }

    public class QueryWithTwoResultsHandler
        : IQueryHandler<QueryWithTwoResults, QueryResult1>,
            IQueryHandler<QueryWithTwoResults, QueryResult2>
    {
        Task<QueryResult1> IQueryHandler<QueryWithTwoResults, QueryResult1>.ExecuteAsync(
            HttpContext context,
            QueryWithTwoResults query
        ) => throw new NotImplementedException();

        Task<QueryResult2> IQueryHandler<QueryWithTwoResults, QueryResult2>.ExecuteAsync(
            HttpContext context,
            QueryWithTwoResults query
        ) => throw new NotImplementedException();
    }

    public class Query3Query4Handler : IQueryHandler<Query3, QueryResult1>, IQueryHandler<Query4, QueryResult2>
    {
        public Task<QueryResult1> ExecuteAsync(HttpContext context, Query3 query) =>
            throw new NotImplementedException();

        public Task<QueryResult2> ExecuteAsync(HttpContext context, Query4 query) =>
            throw new NotImplementedException();
    }
}
