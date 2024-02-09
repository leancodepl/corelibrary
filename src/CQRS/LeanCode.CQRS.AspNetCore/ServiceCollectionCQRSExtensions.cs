using LeanCode.Components;
using LeanCode.Contracts;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.AspNetCore.Registration;
using LeanCode.CQRS.AspNetCore.Serialization;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.CQRS.AspNetCore;

public static class ServiceCollectionCQRSExtensions
{
    public static CQRSServicesBuilder AddCQRS(
        this IServiceCollection serviceCollection,
        TypesCatalog contractsCatalog,
        TypesCatalog handlersCatalog
    )
    {
        serviceCollection.AddSingleton<ISerializer>(_ => new Utf8JsonSerializer(Utf8JsonSerializer.DefaultOptions));

        var objectsSource = new CQRSObjectsRegistrationSource(serviceCollection, new ObjectExecutorFactory());
        objectsSource.AddCQRSObjects(contractsCatalog, handlersCatalog);

        serviceCollection.AddSingleton<ICQRSObjectSource>(objectsSource);
        serviceCollection.AddSingleton(objectsSource);
        serviceCollection.AddSingleton<CQRSMetrics>();

        serviceCollection.AddSingleton<RoleRegistry>();
        serviceCollection.AddScoped<IHasPermissions, DefaultPermissionAuthorizer>();
        serviceCollection.AddScoped<ICommandValidatorResolver, CommandValidatorResolver>();

        return new CQRSServicesBuilder(serviceCollection, objectsSource);
    }

    public static IServiceCollection AddCQRSApiExplorer(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IApiDescriptionProvider, CQRSApiDescriptionProvider>();
        return serviceCollection;
    }
}

public class CQRSServicesBuilder
{
    public IServiceCollection Services { get; }
    private readonly CQRSObjectsRegistrationSource objectsSource;

    internal CQRSServicesBuilder(IServiceCollection services, CQRSObjectsRegistrationSource objectsSource)
    {
        this.Services = services;
        this.objectsSource = objectsSource;
    }

    public CQRSServicesBuilder WithSerializer(ISerializer serializer)
    {
        Services.Replace(new ServiceDescriptor(typeof(ISerializer), serializer));
        return this;
    }

    public CQRSServicesBuilder AddCQRSObjects(TypesCatalog contractsCatalog, TypesCatalog handlersCatalog)
    {
        objectsSource.AddCQRSObjects(contractsCatalog, handlersCatalog);
        return this;
    }

    public CQRSServicesBuilder AddQuery<TQuery, TResult, THandler>()
        where TQuery : IQuery<TResult>
        where THandler : IQueryHandler<TQuery, TResult>
    {
        objectsSource.AddCQRSObject(CQRSObjectKind.Query, typeof(TQuery), typeof(TResult), typeof(THandler));
        return this;
    }

    public CQRSServicesBuilder AddCommand<TCommand, THandler>()
        where TCommand : ICommand
        where THandler : ICommandHandler<TCommand>
    {
        objectsSource.AddCQRSObject(CQRSObjectKind.Command, typeof(TCommand), typeof(CommandResult), typeof(THandler));
        return this;
    }

    public CQRSServicesBuilder AddOperation<TOperation, TResult, THandler>()
        where TOperation : IOperation<TResult>
        where THandler : IOperationHandler<TOperation, TResult>
    {
        objectsSource.AddCQRSObject(CQRSObjectKind.Operation, typeof(TOperation), typeof(TResult), typeof(THandler));
        return this;
    }

    public CQRSServicesBuilder WithLocalCommands(Action<ICQRSApplicationBuilder> configure)
    {
        Services.AddSingleton<Local.ILocalCommandExecutor>(s => new Local.MiddlewareBasedLocalCommandExecutor(
            s,
            s.GetRequiredService<ICQRSObjectSource>(),
            configure
        ));
        return this;
    }

    public CQRSServicesBuilder WithLocalQueries(Action<ICQRSApplicationBuilder> configure)
    {
        Services.AddSingleton<Local.ILocalQueryExecutor>(s => new Local.MiddlewareBasedLocalQueryExecutor(
            s,
            s.GetRequiredService<ICQRSObjectSource>(),
            configure
        ));
        return this;
    }

    public CQRSServicesBuilder WithLocalOperations(Action<ICQRSApplicationBuilder> configure)
    {
        Services.AddSingleton<Local.ILocalOperationExecutor>(s => new Local.MiddlewareBasedLocalOperationExecutor(
            s,
            s.GetRequiredService<ICQRSObjectSource>(),
            configure
        ));
        return this;
    }

    public CQRSServicesBuilder WithKeyedLocalCommands(object? serviceKey, Action<ICQRSApplicationBuilder> configure)
    {
        Services.AddKeyedSingleton<Local.ILocalCommandExecutor>(
            serviceKey,
            (s, _) =>
                new Local.MiddlewareBasedLocalCommandExecutor(s, s.GetRequiredService<ICQRSObjectSource>(), configure)
        );
        return this;
    }

    public CQRSServicesBuilder WithKeyedLocalQueries(object? serviceKey, Action<ICQRSApplicationBuilder> configure)
    {
        Services.AddKeyedSingleton<Local.ILocalQueryExecutor>(
            serviceKey,
            (s, _) =>
                new Local.MiddlewareBasedLocalQueryExecutor(s, s.GetRequiredService<ICQRSObjectSource>(), configure)
        );
        return this;
    }

    public CQRSServicesBuilder WithKeyedLocalOperations(object? serviceKey, Action<ICQRSApplicationBuilder> configure)
    {
        Services.AddKeyedSingleton<Local.ILocalOperationExecutor>(
            serviceKey,
            (s, _) =>
                new Local.MiddlewareBasedLocalOperationExecutor(s, s.GetRequiredService<ICQRSObjectSource>(), configure)
        );
        return this;
    }
}
