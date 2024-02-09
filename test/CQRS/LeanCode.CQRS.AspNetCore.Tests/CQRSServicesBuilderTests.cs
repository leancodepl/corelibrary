using FluentAssertions;
using LeanCode.Components;
using LeanCode.Contracts;
using LeanCode.CQRS.AspNetCore.Registration;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests;

public class CQRSServicesBuilderTests
{
    private readonly ServiceCollection services;
    private readonly CQRSObjectsRegistrationSource registrationSource;
    private readonly CQRSServicesBuilder builder;

    public CQRSServicesBuilderTests()
    {
        services = new();
        registrationSource = new CQRSObjectsRegistrationSource(services, new ObjectExecutorFactory());
        registrationSource.AddCQRSObjects(
            TypesCatalog.Of<CQRSServicesBuilder>(),
            TypesCatalog.Of<CQRSServicesBuilder>()
        );
        builder = new(services, registrationSource);
    }

    [Fact]
    public void Objects_are_not_registered()
    {
        AssertNotRegistered<Query1>();
        AssertNotRegistered<Query2>();
        AssertNotRegistered<Command1>();
        AssertNotRegistered<Operation1>();
    }

    [Fact]
    public void All_objects_are_registered()
    {
        builder
            .AddQuery<Query1, QueryResult1, Query1Handler>()
            .AddQuery<Query2, QueryResult2, Query2Handler>()
            .AddCommand<Command1, Command1Handler>()
            .AddOperation<Operation1, OperationResult1, Operation1Handler>();

        AssertQueryRegistered<Query1, QueryResult1, Query1Handler>();
        AssertQueryRegistered<Query2, QueryResult2, Query2Handler>();
        AssertCommandRegistered<Command1, Command1Handler>();
        AssertOperationRegistered<Operation1, OperationResult1, Operation1Handler>();
    }

    [Fact]
    public void Single_query_is_registered()
    {
        builder.AddQuery<Query1, QueryResult1, Query1Handler>();

        AssertQueryRegistered<Query1, QueryResult1, Query1Handler>();
        AssertNotRegistered<Query2>();
        AssertNotRegistered<Command1>();
        AssertNotRegistered<Operation1>();
    }

    [Fact]
    public void Both_queries_are_registered()
    {
        builder.AddQuery<Query1, QueryResult1, Query1Handler>().AddQuery<Query2, QueryResult2, Query2Handler>();

        AssertQueryRegistered<Query1, QueryResult1, Query1Handler>();
        AssertQueryRegistered<Query2, QueryResult2, Query2Handler>();
        AssertNotRegistered<Command1>();
        AssertNotRegistered<Operation1>();
    }

    [Fact]
    public void Command_is_registered()
    {
        builder.AddCommand<Command1, Command1Handler>();

        AssertNotRegistered<Query1>();
        AssertNotRegistered<Query2>();
        AssertCommandRegistered<Command1, Command1Handler>();
        AssertNotRegistered<Operation1>();
    }

    [Fact]
    public void Operation_is_registered()
    {
        builder.AddOperation<Operation1, OperationResult1, Operation1Handler>();

        AssertNotRegistered<Query1>();
        AssertNotRegistered<Query2>();
        AssertNotRegistered<Command1>();
        AssertOperationRegistered<Operation1, OperationResult1, Operation1Handler>();
    }

    private void AssertQueryRegistered<TQuery, TResult, THandler>()
        where TQuery : IQuery<TResult>
        where THandler : IQueryHandler<TQuery, TResult>
    {
        var cqrsObject = registrationSource.Objects.Should().ContainSingle(o => o.ObjectType == typeof(TQuery)).Which;

        cqrsObject.ObjectKind.Should().Be(CQRSObjectKind.Query);
        cqrsObject.ResultType.Should().Be(typeof(TResult));
        cqrsObject.HandlerType.Should().Be(typeof(THandler));
    }

    private void AssertCommandRegistered<TCommand, THandler>()
        where TCommand : ICommand
        where THandler : ICommandHandler<TCommand>
    {
        var cqrsObject = registrationSource.Objects.Should().ContainSingle(o => o.ObjectType == typeof(TCommand)).Which;

        cqrsObject.ObjectKind.Should().Be(CQRSObjectKind.Command);
        cqrsObject.ResultType.Should().Be(typeof(CommandResult));
        cqrsObject.HandlerType.Should().Be(typeof(THandler));
    }

    private void AssertOperationRegistered<TOperation, TResult, THandler>()
        where TOperation : IOperation<TResult>
        where THandler : IOperationHandler<TOperation, TResult>
    {
        var cqrsObject = registrationSource
            .Objects.Should()
            .ContainSingle(o => o.ObjectType == typeof(TOperation))
            .Which;

        cqrsObject.ObjectKind.Should().Be(CQRSObjectKind.Operation);
        cqrsObject.ResultType.Should().Be(typeof(TResult));
        cqrsObject.HandlerType.Should().Be(typeof(THandler));
    }

    private void AssertNotRegistered<T>()
    {
        registrationSource.Objects.Should().NotContain(o => o.ObjectType == typeof(T));
    }

    private sealed class Query1 : IQuery<QueryResult1> { }

    private sealed class QueryResult1 { }

    private sealed class Query2 : IQuery<QueryResult2> { }

    private sealed class QueryResult2 { }

    private sealed class Command1 : ICommand { }

    private sealed class Operation1 : IOperation<OperationResult1> { }

    private sealed class OperationResult1 { }

    private sealed class Query1Handler : IQueryHandler<Query1, QueryResult1>
    {
        public Task<QueryResult1> ExecuteAsync(HttpContext context, Query1 query) =>
            throw new NotImplementedException();
    }

    private sealed class Query2Handler : IQueryHandler<Query2, QueryResult2>
    {
        public Task<QueryResult2> ExecuteAsync(HttpContext context, Query2 query) =>
            throw new NotImplementedException();
    }

    private sealed class Command1Handler : ICommandHandler<Command1>
    {
        public Task ExecuteAsync(HttpContext context, Command1 command) => throw new NotImplementedException();
    }

    private sealed class Operation1Handler : IOperationHandler<Operation1, OperationResult1>
    {
        public Task<OperationResult1> ExecuteAsync(HttpContext context, Operation1 operation) =>
            throw new NotImplementedException();
    }
}
