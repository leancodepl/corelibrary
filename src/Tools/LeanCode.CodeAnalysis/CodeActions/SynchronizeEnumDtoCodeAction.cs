using System.Globalization;
using LeanCode.CodeAnalysis.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace LeanCode.CodeAnalysis.CodeActions;

public class SynchronizeEnumDtoCodeAction : CodeAction
{
    private const string DtoSuffix = "DTO";
    private const string ExcludeMembersAttributeName = "LeanCode.CodeAnalysis.ExcludeMembersAttribute";
    private const string IgnoreEnumValueAttributeName = "LeanCode.CodeAnalysis.IgnoreEnumValueAttribute";
    private const string EnumValueCorrespondsAttributeName = "LeanCode.CodeAnalysis.EnumValueCorrespondsAttribute";

    private readonly Document document;
    private readonly TextSpan enumSpan;

    public override string Title => "Synchronize DTO enum with base enum";
    public override string EquivalenceKey => Title;

    public SynchronizeEnumDtoCodeAction(Document document, TextSpan enumSpan)
    {
        this.document = document;
        this.enumSpan = enumSpan;
    }

    protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        var model = await document.GetSemanticModelAsync(cancellationToken);

        if (root is null || model is null)
        {
            return document;
        }

        var enumDeclaration = root.FindNode(enumSpan).FirstAncestorOrSelf<EnumDeclarationSyntax>();
        if (enumDeclaration is null)
        {
            return document;
        }

        if (
            model.GetDeclaredSymbol(enumDeclaration, cancellationToken) is not INamedTypeSymbol enumSymbol
            || !enumSymbol.Name.EndsWith(DtoSuffix, StringComparison.Ordinal)
        )
        {
            return document;
        }

        var baseEnumName = enumSymbol.Name[..^DtoSuffix.Length];
        var baseEnum = FindBaseEnum(enumSymbol, baseEnumName);

        if (baseEnum is null)
        {
            return document;
        }

        var synchronizedEnum = GenerateSynchronizedEnum(enumDeclaration, enumSymbol, baseEnum, model);
        var newRoot = root.ReplaceNode(enumDeclaration, synchronizedEnum);

        return document.WithSyntaxRoot(newRoot);
    }

    private static INamedTypeSymbol? FindBaseEnum(INamedTypeSymbol dtoEnum, string baseEnumName)
    {
        var sameNamespaceEnum = dtoEnum
            .ContainingNamespace.GetTypeMembers(baseEnumName)
            .FirstOrDefault(t => t.TypeKind == TypeKind.Enum);

        if (sameNamespaceEnum is not null)
        {
            return sameNamespaceEnum;
        }

        return FindEnumInCompilation(dtoEnum.ContainingAssembly.GlobalNamespace, baseEnumName);
    }

    private static INamedTypeSymbol? FindEnumInCompilation(INamespaceSymbol namespaceSymbol, string enumName)
    {
        var enumType = namespaceSymbol.GetTypeMembers(enumName).FirstOrDefault(t => t.TypeKind == TypeKind.Enum);

        if (enumType is not null)
        {
            return enumType;
        }

        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            var result = FindEnumInCompilation(childNamespace, enumName);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    private static EnumDeclarationSyntax GenerateSynchronizedEnum(
        EnumDeclarationSyntax originalEnum,
        INamedTypeSymbol enumSymbol,
        INamedTypeSymbol baseEnum,
        SemanticModel model
    )
    {
        var excludedMembers = GetExcludedMembers(enumSymbol, baseEnum);

        var existingMembers = originalEnum.Members.ToDictionary(m => m.Identifier.ValueText, m => m);

        var newMembers = new List<EnumMemberDeclarationSyntax>();

        // Existing DTO members that have special attributes (Ignore or Corresponds) should be preserved as-is
        foreach (var existingMember in existingMembers.Values)
        {
            if (HasSpecialAttributes(existingMember, model))
            {
                newMembers.Add(existingMember);
            }
        }

        var baseMembers = baseEnum
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.IsStatic && f.HasConstantValue && !excludedMembers.Contains(f.Name))
            .OrderBy(f => Convert.ToInt64(f.ConstantValue, CultureInfo.InvariantCulture))
            .ToList();

        foreach (var baseMember in baseMembers)
        {
            if (IsBaseMemberCoveredByCorrespondsAttribute(baseMember, existingMembers, model))
            {
                continue;
            }

            if (existingMembers.TryGetValue(baseMember.Name, out var existingMember))
            {
                if (HasEnumValueCorrespondsAttribute(existingMember, model))
                {
                    continue;
                }

                var currentValue = GetEnumMemberValue(existingMember);
                var expectedValue = Convert.ToInt64(baseMember.ConstantValue, CultureInfo.InvariantCulture);

                if (currentValue != expectedValue)
                {
                    var newMember = existingMember.WithEqualsValue(
                        SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                SyntaxFactory.Literal((int)expectedValue)
                            )
                        )
                    );
                    newMembers.Add(newMember);
                }
                else
                {
                    newMembers.Add(existingMember);
                }
            }
            else
            {
                var newMember = SyntaxFactory
                    .EnumMemberDeclaration(baseMember.Name)
                    .WithEqualsValue(
                        SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                SyntaxFactory.Literal(
                                    (int)Convert.ToInt64(baseMember.ConstantValue, CultureInfo.InvariantCulture)
                                )
                            )
                        )
                    );

                newMembers.Add(newMember);
            }
        }

        newMembers = newMembers.OrderBy(GetEnumMemberValue).ToList();

        return originalEnum.WithMembers(SyntaxFactory.SeparatedList(newMembers));
    }

    private static HashSet<string> GetExcludedMembers(INamedTypeSymbol dtoEnum, INamedTypeSymbol baseEnum)
    {
        var excludedMembers = new HashSet<string>();

        var excludeAttr = dtoEnum
            .GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.GetFullNamespaceName() == ExcludeMembersAttributeName);

        if (excludeAttr?.ConstructorArguments.Length > 0)
        {
            foreach (var arg in excludeAttr.ConstructorArguments)
            {
                if (arg.Kind == TypedConstantKind.Array && arg.Values.Length > 0)
                {
                    foreach (var value in arg.Values)
                    {
                        if (value.Value is not null)
                        {
                            var memberName = GetEnumMemberNameByValue(baseEnum, value.Value);
                            if (memberName is not null)
                            {
                                excludedMembers.Add(memberName);
                            }
                        }
                    }
                }
                else if (arg.Value is not null)
                {
                    var memberName = GetEnumMemberNameByValue(baseEnum, arg.Value);
                    if (memberName is not null)
                    {
                        excludedMembers.Add(memberName);
                    }
                }
            }
        }

        return excludedMembers;
    }

    private static string? GetEnumMemberNameByValue(INamedTypeSymbol enumSymbol, object value)
    {
        return enumSymbol
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.IsStatic && f.HasConstantValue)
            .FirstOrDefault(f => AreEnumValuesEqual(f.ConstantValue, value))
            ?.Name;
    }

    private static bool AreEnumValuesEqual(object? value1, object? value2)
    {
        if (value1 is null && value2 is null)
        {
            return true;
        }
        if (value1 is null || value2 is null)
        {
            return false;
        }

        var type1 = value1.GetType();
        var type2 = value2.GetType();

        if (!type1.IsEnum || !type2.IsEnum)
        {
            return value1.Equals(value2);
        }

        try
        {
            var underlyingValue1 = Convert.ToInt64(value1, CultureInfo.InvariantCulture);
            var underlyingValue2 = Convert.ToInt64(value2, CultureInfo.InvariantCulture);
            return underlyingValue1 == underlyingValue2;
        }
        catch (OverflowException)
        {
            return false;
        }
        catch (InvalidCastException)
        {
            return false;
        }
    }

    private static long GetEnumMemberValue(EnumMemberDeclarationSyntax member)
    {
        if (
            member.EqualsValue?.Value is LiteralExpressionSyntax literal
            && literal.Token.IsKind(SyntaxKind.NumericLiteralToken)
        )
        {
            if (long.TryParse(literal.Token.ValueText, out var value))
            {
                return value;
            }
        }

        return 0; // Default value if not specified
    }

    private static bool HasSpecialAttributes(EnumMemberDeclarationSyntax member, SemanticModel model)
    {
        foreach (var attributeList in member.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var symbolInfo = model.GetSymbolInfo(attribute);
                if (symbolInfo.Symbol is IMethodSymbol method)
                {
                    var attributeTypeName = method.ContainingType.GetFullNamespaceName();
                    if (
                        attributeTypeName == IgnoreEnumValueAttributeName
                        || attributeTypeName == EnumValueCorrespondsAttributeName
                    )
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static bool HasEnumValueCorrespondsAttribute(EnumMemberDeclarationSyntax member, SemanticModel model)
    {
        foreach (var attributeList in member.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var symbolInfo = model.GetSymbolInfo(attribute);
                if (symbolInfo.Symbol is IMethodSymbol method)
                {
                    var attributeTypeName = method.ContainingType.GetFullNamespaceName();
                    if (attributeTypeName == EnumValueCorrespondsAttributeName)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static bool IsBaseMemberCoveredByCorrespondsAttribute(
        IFieldSymbol baseMember,
        Dictionary<string, EnumMemberDeclarationSyntax> existingMembers,
        SemanticModel model
    )
    {
        var baseMemberValue = Convert
            .ToInt64(baseMember.ConstantValue, CultureInfo.InvariantCulture)
            .ToString(CultureInfo.InvariantCulture);

        foreach (var existingMember in existingMembers.Values)
        {
            if (!HasEnumValueCorrespondsAttribute(existingMember, model))
            {
                continue;
            }

            var correspondingValues = GetCorrespondingValuesFromAttribute(existingMember, model);
            if (correspondingValues.Contains(baseMemberValue))
            {
                return true;
            }
        }

        return false;
    }

    private static HashSet<string> GetCorrespondingValuesFromAttribute(
        EnumMemberDeclarationSyntax member,
        SemanticModel model
    )
    {
        var correspondingValues = new HashSet<string>();

        foreach (var attributeList in member.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var symbolInfo = model.GetSymbolInfo(attribute);
                if (symbolInfo.Symbol is IMethodSymbol method)
                {
                    var attributeTypeName = method.ContainingType.GetFullNamespaceName();
                    if (attributeTypeName == EnumValueCorrespondsAttributeName)
                    {
                        if (attribute.ArgumentList?.Arguments.Count > 0)
                        {
                            foreach (var argument in attribute.ArgumentList.Arguments)
                            {
                                if (argument.Expression is LiteralExpressionSyntax literal)
                                {
                                    if (literal.Token.IsKind(SyntaxKind.NumericLiteralToken))
                                    {
                                        var value = literal.Token.ValueText;
                                        correspondingValues.Add(value);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return correspondingValues;
    }
}
