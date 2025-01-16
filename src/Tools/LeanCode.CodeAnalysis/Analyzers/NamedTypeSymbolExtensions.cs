using Microsoft.CodeAnalysis;

namespace LeanCode.CodeAnalysis.Analyzers;

public static class NamedTypeSymbolExtensions
{
    private const string AuthorizeWhenTypeName = "LeanCode.Contracts.Security.AuthorizeWhenAttribute";
    private const string AllowUnauthorizedTypeName = "LeanCode.Contracts.Security.AllowUnauthorizedAttribute";

    public static bool ImplementsInterfaceOrBaseClass(this INamedTypeSymbol? typeSymbol, string type)
    {
        if (typeSymbol == null)
        {
            return false;
        }
        else if (typeSymbol.GetTypeAndBaseClasses().Any(t => t.GetFullNamespaceName() == type))
        {
            return true;
        }

        foreach (var @interface in typeSymbol.AllInterfaces)
        {
            if (@interface.GetFullNamespaceName() == type)
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<INamedTypeSymbol> GetTypeAndBaseClasses(this INamedTypeSymbol typeSymbol)
    {
        var type = typeSymbol;

        while (type is not null)
        {
            yield return type;
            type = type.BaseType;
        }
    }

    public static string GetFullNamespaceName(this INamedTypeSymbol type)
    {
        return $"{type.ContainingNamespace}.{type.MetadataName}";
    }

    public static bool HasAuthorizationAttribute(this INamedTypeSymbol type)
    {
        var attributes = type.GetAttributes();
        if (
            attributes.Any(attr =>
                attr.AttributeClass is object
                && (
                    attr.AttributeClass.ImplementsInterfaceOrBaseClass(AuthorizeWhenTypeName)
                    || attr.AttributeClass.ImplementsInterfaceOrBaseClass(AllowUnauthorizedTypeName)
                )
            )
        )
        {
            return true;
        }

        return type.BaseType != null && HasAuthorizationAttribute(type.BaseType);
    }
}
