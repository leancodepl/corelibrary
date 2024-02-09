using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using LeanCode.Components;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.AspNetCore.Local;
using LeanCode.CQRS.AspNetCore.Registration;
using LeanCode.CQRS.AspNetCore.Serialization;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests;

public class ServiceProviderRegistrationExtensionsTests
{
    private static readonly TypesCatalog ThisCatalog = TypesCatalog.Of<ServiceProviderRegistrationExtensionsTests>();

    [Fact]
    public void Registers_base_services()
    {
        var sp = BuildProvider();

        sp.Should().HaveService<ICQRSObjectSource>();
        sp.Should().HaveService<CQRSMetrics>();
        sp.Should().HaveService<ISerializer>();
        sp.Should().HaveService<RoleRegistry>();
        sp.Should().HaveService<IHasPermissions>();
        sp.Should().HaveService<ICommandValidatorResolver>();
    }

    [Fact]
    public void Registers_handlers()
    {
        var sp = BuildProvider();

        sp.Should().HaveService<Local.LocalCommandHandler>();
        sp.Should().HaveService<Local.LocalQueryHandler>();
        sp.Should().HaveService<Local.RemoteQueryHandler>();
    }

    [Fact]
    public void Does_not_register_local_executors_by_default()
    {
        var sp = BuildProvider();

        sp.Should().NotHaveService<ILocalCommandExecutor>();
        sp.Should().NotHaveService<ILocalQueryExecutor>();
        sp.Should().NotHaveService<ILocalOperationExecutor>();
    }

    [Fact]
    public void Registers_local_executors_on_request()
    {
        var sp = BuildProvider(c =>
            c.WithLocalCommands(_ => { }).WithLocalQueries(_ => { }).WithLocalOperations(_ => { })
        );

        sp.Should().HaveService<ILocalCommandExecutor>();
        sp.Should().HaveService<ILocalQueryExecutor>();
        sp.Should().HaveService<ILocalOperationExecutor>();
    }

    [Fact]
    public void Registers_keyed_local_executors_on_request()
    {
        var sp = BuildProvider(c =>
            c.WithKeyedLocalCommands("commands", _ => { })
                .WithKeyedLocalQueries("queries", _ => { })
                .WithKeyedLocalOperations("operations", _ => { })
        );

        sp.Should().HaveKeyedService<ILocalCommandExecutor>("commands");
        sp.Should().HaveKeyedService<ILocalQueryExecutor>("queries");
        sp.Should().HaveKeyedService<ILocalOperationExecutor>("operations");
    }

    [Fact]
    public void Replaces_serializer_service()
    {
        var serializer = new CustomSerializer();
        var sp = BuildProvider(c => c.WithSerializer(serializer));

        sp.GetRequiredService<ISerializer>().Should().BeSameAs(serializer);
    }

    private static ServiceProvider BuildProvider(Action<CQRSServicesBuilder>? configure = null)
    {
        var collection = new ServiceCollection();
        var builder = collection.AddCQRS(ThisCatalog, ThisCatalog);
        configure?.Invoke(builder);
        return collection.BuildServiceProvider();
    }

    internal sealed class CustomSerializer : ISerializer
    {
        public ValueTask<object?> DeserializeAsync(
            Stream utf8Json,
            Type returnType,
            CancellationToken cancellationToken
        ) => throw new NotImplementedException();

        public Task SerializeAsync(
            Stream utf8Json,
            object value,
            Type inputType,
            CancellationToken cancellationToken
        ) => throw new NotImplementedException();
    }
}

file sealed class ServiceProviderAssertions : ReferenceTypeAssertions<ServiceProvider, ServiceProviderAssertions>
{
    protected override string Identifier => "services";

    public ServiceProviderAssertions(ServiceProvider subject)
        : base(subject) { }

    public AndConstraint<ServiceProviderAssertions> HaveService<T>(string because = "", params object[] becauseArgs)
    {
        Execute
            .Assertion.BecauseOf(because, becauseArgs)
            .ForCondition(Subject.GetRequiredService<IServiceProviderIsService>().IsService(typeof(T)))
            .FailWith("Expected to have {0} registered{reason}", typeof(T));
        return new AndConstraint<ServiceProviderAssertions>(this);
    }

    public AndConstraint<ServiceProviderAssertions> HaveKeyedService<T>(
        object? serviceKey,
        string because = "",
        params object[] becauseArgs
    )
    {
        Execute
            .Assertion.BecauseOf(because, becauseArgs)
            .ForCondition(
                Subject.GetRequiredService<IServiceProviderIsKeyedService>().IsKeyedService(typeof(T), serviceKey)
            )
            .FailWith("Expected to have {0} registered as key {1}{reason}", typeof(T), serviceKey);
        return new AndConstraint<ServiceProviderAssertions>(this);
    }

    public AndConstraint<ServiceProviderAssertions> NotHaveService<T>(string because = "", params object[] becauseArgs)
    {
        Execute
            .Assertion.BecauseOf(because, becauseArgs)
            .ForCondition(!Subject.GetRequiredService<IServiceProviderIsService>().IsService(typeof(T)))
            .FailWith("Expected to have {0} registered{reason}", typeof(T));
        return new AndConstraint<ServiceProviderAssertions>(this);
    }
}

file static class ServiceProviderExtensions
{
    public static ServiceProviderAssertions Should(this ServiceProvider sp) => new(sp);
}
