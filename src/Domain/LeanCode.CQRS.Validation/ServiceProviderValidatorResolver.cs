using System.Collections.Concurrent;
using System.Reflection;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.Validation;

public class ServiceProviderValidatorResolver : ICommandValidatorResolver
{
    private static readonly MethodInfo ResolveValidatorMethod =
        typeof(ServiceProviderValidatorResolver).GetMethod(
            nameof(ResolveValidator),
            BindingFlags.Static | BindingFlags.NonPublic
        ) ?? throw new InvalidOperationException("Cannot get ResolveValidator method");

    private static readonly ConcurrentDictionary<Type, Func<IServiceProvider, ICommandValidatorWrapper?>> FactoryCache =
        new();

    private readonly IServiceProvider serviceProvider;

    public ServiceProviderValidatorResolver(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public ICommandValidatorWrapper? FindCommandValidator(Type commandType)
    {
        var factory = FactoryCache.GetOrAdd(commandType, ResolveValidatorFactory);
        return factory(serviceProvider);
    }

    private static Func<IServiceProvider, ICommandValidatorWrapper?> ResolveValidatorFactory(Type commandType)
    {
        return ResolveValidatorMethod
            .MakeGenericMethod(commandType)
            .CreateDelegate<Func<IServiceProvider, ICommandValidatorWrapper?>>();
    }

    private static ICommandValidatorWrapper? ResolveValidator<TCommand>(IServiceProvider sp)
        where TCommand : ICommand
    {
        var validator = sp.GetService<ICommandValidator<TCommand>>();
        return validator is not null ? new CommandValidatorWrapper<TCommand>(validator) : null;
    }
}

public class CommandValidatorWrapper<TCommand> : ICommandValidatorWrapper
    where TCommand : ICommand
{
    private readonly ICommandValidator<TCommand> validator;

    public CommandValidatorWrapper(ICommandValidator<TCommand> validator)
    {
        this.validator = validator;
    }

    public Task<ValidationResult> ValidateAsync(HttpContext appContext, ICommand command)
    {
        return validator.ValidateAsync(appContext, (TCommand)command);
    }
}
