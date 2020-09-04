using System.Linq;
using Microsoft.CodeAnalysis;

namespace LeanCode.CodeAnalysis.Analyzers
{
    public static class NamedTypeSymbolExtensions
    {
        public const string AuthorizeWhenTypeName = "LeanCode.CQRS.Security.AuthorizeWhenAttribute";
        public const string AllowUnauthorizedTypeName = "LeanCode.CQRS.Security.AllowUnauthorizedAttribute";
        public const string CommandTypeName = "LeanCode.CQRS.ICommand";
        public const string QueryTypeName = "LeanCode.CQRS.IQuery";
        public const string ValidatorTypeName = "FluentValidation.IValidator`1";

        public static bool ImplementsInterfaceOrBaseClass(this INamedTypeSymbol typeSymbol, string type)
        {
            if (typeSymbol == null)
            {
                return false;
            }
            else if (typeSymbol.GetFullNamespaceName() == type)
            {
                return true;
            }
            else if (typeSymbol.BaseType?.GetFullNamespaceName() == type)
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

        public static bool HasAuthorizationAttribute(this INamedTypeSymbol type)
        {
            var attributes = type.GetAttributes();
            if (attributes.Any(attr =>
                attr.AttributeClass.ImplementsInterfaceOrBaseClass(AuthorizeWhenTypeName) ||
                attr.AttributeClass.ImplementsInterfaceOrBaseClass(AllowUnauthorizedTypeName)))
            {
                return true;
            }

            return type.BaseType != null && HasAuthorizationAttribute(type.BaseType);
        }

        public static bool IsCommand(this INamedTypeSymbol type)
        {
            return type.TypeKind != TypeKind.Interface && type.ImplementsInterfaceOrBaseClass(CommandTypeName) && !type.IsAbstract;
        }

        public static bool IsQuery(this INamedTypeSymbol type)
        {
            return type.TypeKind != TypeKind.Interface && type.ImplementsInterfaceOrBaseClass(QueryTypeName) && !type.IsAbstract;
        }

        public static bool IsValidator(this INamedTypeSymbol type)
        {
            return type.AllInterfaces
                .Where(i => i.GetFullNamespaceName() == ValidatorTypeName)
                .Any();
        }

        public static bool IsObjectValidator(this ITypeSymbol type, INamedTypeSymbol validatedObj)
        {
            return type.AllInterfaces
                .Where(i => i.GetFullNamespaceName() == ValidatorTypeName)
                .Where(i => SymbolEqualityComparer.Default.Equals(i.TypeArguments.First(), validatedObj))
                .Any();
        }
    }
}
