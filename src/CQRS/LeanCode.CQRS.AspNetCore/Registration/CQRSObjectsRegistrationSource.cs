using System.Reflection;
using LeanCode.Components;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.AspNetCore.Registration;

internal class CQRSObjectsRegistrationSource
{
    private readonly IServiceCollection services;
    private readonly HashSet<CQRSObjectMetadata> objects = new();

    public IReadOnlySet<CQRSObjectMetadata> Objects => objects;

    public CQRSObjectsRegistrationSource(IServiceCollection services)
    {
        this.services = services;
    }

    public void AddCQRSObjects(TypesCatalog contractsCatalog, TypesCatalog handlersCatalog)
    {
        var contracts = contractsCatalog.Assemblies
            .SelectMany(a => a.DefinedTypes)
            .Where(t => IsCommand(t) || IsQuery(t) || IsOperation(t));

        var handlers = handlersCatalog.Assemblies
            .SelectMany(a => a.DefinedTypes)
            .SelectMany(EnumerateHandledObjects)
            .ToLookup(h => h.ObjectType);

        foreach (var contract in contracts)
        {
            if (!ValidateContractType(contract))
            {
                continue;
            }

            var handlerCandidates = handlers[contract];

            if (handlerCandidates.Count() != 1)
            {
                continue;
            }

            var handler = handlerCandidates.Single();

            var metadata = new CQRSObjectMetadata(
                handler.ObjectKind,
                objectType: contract,
                resultType: handler.ResultType,
                handlerType: handler.HandlerType
            );

            var added = objects.Add(metadata);

            if (added)
            {
                services.AddCQRSHandler(metadata);
            }
        }
    }

    private static bool ValidateContractType(TypeInfo type)
    {
        var implementedContractInterfaces = type.ImplementedInterfaces.Where(
            i => IsGenericType(i, typeof(IQuery<>)) || i == typeof(ICommand) || IsGenericType(i, typeof(IOperation<>))
        );

        return implementedContractInterfaces.Count() == 1;
    }

    private static IEnumerable<HandlerDefinition> EnumerateHandledObjects(TypeInfo t)
    {
        var handledQueries = t.ImplementedInterfaces
            .Where(qh => IsGenericType(qh, typeof(IQueryHandler<,>)))
            .Select(
                qh =>
                    new HandlerDefinition(
                        CQRSObjectKind.Query,
                        t,
                        qh.GenericTypeArguments[0],
                        qh.GenericTypeArguments[1]
                    )
            );

        var handledCommands = t.ImplementedInterfaces
            .Where(ch => IsGenericType(ch, typeof(ICommandHandler<>)))
            .Select(
                ch =>
                    new HandlerDefinition(CQRSObjectKind.Command, t, ch.GenericTypeArguments[0], typeof(CommandResult))
            );

        var handledOperations = t.ImplementedInterfaces
            .Where(oh => IsGenericType(oh, typeof(IOperationHandler<,>)))
            .Select(
                oh =>
                    new HandlerDefinition(
                        CQRSObjectKind.Operation,
                        t,
                        oh.GenericTypeArguments[0],
                        oh.GenericTypeArguments[1]
                    )
            );

        return handledQueries.Concat(handledCommands).Concat(handledOperations);
    }

    private static bool IsQuery(TypeInfo type)
    {
        return ImplementsGenericType(type, typeof(IQuery<>));
    }

    private static bool IsCommand(TypeInfo type)
    {
        return type.ImplementedInterfaces.Contains(typeof(ICommand));
    }

    private static bool IsOperation(TypeInfo type)
    {
        return ImplementsGenericType(type, typeof(IOperation<>));
    }

    private static bool ImplementsGenericType(TypeInfo type, Type implementedType) =>
        type.ImplementedInterfaces.Any(
            i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == implementedType
        );

    private static bool IsGenericType(Type type, Type genericType)
    {
        return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == genericType;
    }

    private class HandlerDefinition
    {
        public CQRSObjectKind ObjectKind { get; }
        public Type HandlerType { get; }
        public Type ObjectType { get; }
        public Type ResultType { get; }

        public HandlerDefinition(CQRSObjectKind objectKind, Type handlerType, Type objectType, Type resultType)
        {
            ObjectKind = objectKind;
            HandlerType = handlerType;
            ObjectType = objectType;
            ResultType = resultType;
        }
    }
}
