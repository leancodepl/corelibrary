using System.Linq;
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


        private const string AuthorizeWhenTypeName = "LeanCode.CQRS.Security.AuthorizeWhenAttribute";
        private const string AllowUnauthorizedTypeName = "LeanCode.CQRS.Security.AllowUnauthorizedAttribute";

        public static bool HasAuthorizationAttribute(this INamedTypeSymbol type)
        {
            var attributes = type.GetAttributes();
            if (attributes.Any(attr =>
                attr.AttributeClass.ImplementsInterfaceOrBaseClass(AuthorizeWhenTypeName) ||
                attr.AttributeClass.ImplementsInterfaceOrBaseClass(AllowUnauthorizedTypeName)))
            {
                return true;
            }
            return type.BaseType != null ? HasAuthorizationAttribute(type.BaseType) : false;
        }
    }
}
