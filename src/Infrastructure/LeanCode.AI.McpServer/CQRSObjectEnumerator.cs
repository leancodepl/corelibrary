using System.Reflection;
using LeanCode.Components;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;

namespace LeanCode.AI.McpServer;

internal static class CQRSObjectEnumeratorExtensions
{
    public static IEnumerable<(TypeInfo Contract, CQRSObjectKind Kind, Type Result)> GetCQRSObjects(
        this TypesCatalog contractsCatalog
    )
    {
        var contracts = contractsCatalog
            .Assemblies.SelectMany(a => a.DefinedTypes)
            .Where(t => IsCommand(t) || IsQuery(t) || IsOperation(t));

        foreach (var contract in contracts)
        {
            if (GetContractObjectKindAndResultType(contract) is var (kind, result))
            {
                yield return (contract, kind, result);
            }
        }
    }

    private static (CQRSObjectKind, Type)? GetContractObjectKindAndResultType(TypeInfo type)
    {
        var commands = type.ImplementedInterfaces.Where(i => i == typeof(ICommand)).ToList();
        var queries = type.ImplementedInterfaces.Where(i => IsGenericType(i, typeof(IQuery<>))).ToList();
        var operations = type.ImplementedInterfaces.Where(i => IsGenericType(i, typeof(IOperation<>))).ToList();

        if (commands.Count + queries.Count + operations.Count != 1)
        {
            return null;
        }

        return (commands, queries, operations) switch
        {
            ([{ }], [], []) => (CQRSObjectKind.Command, typeof(CommandResult)),
            ([], [{ } query], []) => (CQRSObjectKind.Query, query.GetGenericArguments().Single()),
            ([], [], [{ } operation]) => (CQRSObjectKind.Operation, operation.GetGenericArguments().Single()),
            _ => null,
        };
    }

    private static bool IsCommand(TypeInfo type)
    {
        return type.ImplementedInterfaces.Contains(typeof(ICommand));
    }

    private static bool IsQuery(TypeInfo type)
    {
        return ImplementsGenericType(type, typeof(IQuery<>));
    }

    private static bool IsOperation(TypeInfo type)
    {
        return ImplementsGenericType(type, typeof(IOperation<>));
    }

    private static bool ImplementsGenericType(TypeInfo type, Type implementedType)
    {
        return type.ImplementedInterfaces.Any(i => IsGenericType(i, implementedType));
    }

    private static bool IsGenericType(Type type, Type genericType)
    {
        return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == genericType;
    }
}
