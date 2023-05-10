using System.Reflection;
using LeanCode.Components;
using LeanCode.Contracts;

namespace LeanCode.CQRS.RemoteHttp.Server;

public static class ObjectsFinder
{
    public static IEnumerable<CQRSObjectMetadata> FindCqrsObjects(TypesCatalog catalog)
    {
        var types = catalog.Assemblies.SelectMany(a => a.DefinedTypes);

        var queries = types.Where(IsQuery).Select(q => new CQRSObjectMetadata(q, CQRSObjectKind.Query));
        var commands = types.Where(IsCommand).Select(q => new CQRSObjectMetadata(q, CQRSObjectKind.Command));
        var operations = types.Where(IsOperation).Select(q => new CQRSObjectMetadata(q, CQRSObjectKind.Operation));

        return queries.Concat(commands).Concat(operations);
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
        return ImplementsGenericType(type, typeof(IOperation));
    }

    private static bool ImplementsGenericType(TypeInfo type, Type implementedType) =>
        type.ImplementedInterfaces.Any(i =>
            i.IsConstructedGenericType && i.GetGenericTypeDefinition() == implementedType);
}
