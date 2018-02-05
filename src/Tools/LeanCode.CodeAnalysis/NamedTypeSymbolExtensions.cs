using Microsoft.CodeAnalysis;

namespace LeanCode.CodeAnalysis
{
    public static class NamedTypeSymbolExtensions
    {
        public static bool ImplementsInterfaceOrBaseClass(this INamedTypeSymbol typeSymbol, string type)
        {
            if (typeSymbol == null)
            {
                return false;
            }

            if (typeSymbol.GetFullNamespaceName() == type)
            {
                return true;
            }

            if (typeSymbol.BaseType.GetFullNamespaceName() == type)
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

        public static string GetFullNamespaceName(this INamedTypeSymbol type)
        {
            return $"{type.ContainingNamespace}.{type.MetadataName}";
        }

    }
}
