using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LeanCode.CodeAnalysis.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnumDtoConsistencyAnalyzer : DiagnosticAnalyzer
{
    private const string Category = "Design";
    private const string DtoSuffix = "DTO";
    private const string IgnoreEnumDtoAttributeName = "LeanCode.CodeAnalysis.IgnoreEnumDtoAttribute";
    private const string ExcludeMembersAttributeName = "LeanCode.CodeAnalysis.ExcludeMembersAttribute";
    private const string IgnoreEnumValueAttributeName = "LeanCode.CodeAnalysis.IgnoreEnumValueAttribute";
    private const string EnumValueCorrespondsAttributeName = "LeanCode.CodeAnalysis.EnumValueCorrespondsAttribute";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticsIds.EnumDtoShouldMatchBaseEnum,
        "DTO enum should match its base enum",
        "DTO enum '{0}' should have the same members as its base enum '{1}'. Missing: [{2}]. Extra: [{3}]. Value mismatches: [{4}].",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "DTO enums should have the same members (names and values) as their corresponding base enums, unless explicitly configured with attributes."
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.RegisterSymbolAction(AnalyzeEnum, SymbolKind.NamedType);
    }

    private static void AnalyzeEnum(SymbolAnalysisContext context)
    {
        var enumSymbol = (INamedTypeSymbol)context.Symbol;

        if (enumSymbol.TypeKind != TypeKind.Enum)
        {
            return;
        }

        if (!enumSymbol.Name.EndsWith(DtoSuffix, StringComparison.Ordinal))
        {
            return;
        }

        if (HasIgnoreEnumDtoAttribute(enumSymbol))
        {
            return;
        }

        var baseEnumName = enumSymbol.Name[..^DtoSuffix.Length];
        var baseEnum = FindBaseEnum(enumSymbol, baseEnumName);

        if (baseEnum == null)
        {
            return;
        }

        var analysis = AnalyzeEnumConsistency(enumSymbol, baseEnum);

        if (analysis.HasIssues)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                enumSymbol.Locations[0],
                enumSymbol.Name,
                baseEnum.Name,
                string.Join(", ", analysis.MissingMembers),
                string.Join(", ", analysis.ExtraMembers),
                string.Join(", ", analysis.ValueMismatches)
            );

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool HasIgnoreEnumDtoAttribute(INamedTypeSymbol enumSymbol)
    {
        return enumSymbol
            .GetAttributes()
            .Any(attr => attr.AttributeClass?.GetFullNamespaceName() == IgnoreEnumDtoAttributeName);
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

    private static EnumConsistencyAnalysis AnalyzeEnumConsistency(INamedTypeSymbol dtoEnum, INamedTypeSymbol baseEnum)
    {
        var analysis = new EnumConsistencyAnalysis();

        var excludedMembers = GetExcludedMembers(dtoEnum, baseEnum);

        var baseMembers = baseEnum
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.IsStatic && f.HasConstantValue && !excludedMembers.Contains(f.Name))
            .ToDictionary(f => f.Name, f => f);

        var dtoMembers = dtoEnum
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.IsStatic && f.HasConstantValue)
            .ToDictionary(f => f.Name, f => f);

        var accountedBaseMembers = new HashSet<string>();

        foreach (var dtoMember in dtoMembers.Values)
        {
            if (HasIgnoreEnumValueAttribute(dtoMember))
            {
                continue;
            }

            var correspondsAttr = GetEnumValueCorrespondsAttribute(dtoMember);

            if (correspondsAttr is not null)
            {
                var correspondingNames = GetCorrespondingEnumNames(correspondsAttr, baseEnum);

                foreach (var correspondingName in correspondingNames)
                {
                    if (baseMembers.TryGetValue(correspondingName, out var baseMember))
                    {
                        accountedBaseMembers.Add(correspondingName);
                    }
                }
            }
            else
            {
                if (baseMembers.TryGetValue(dtoMember.Name, out var baseMember))
                {
                    accountedBaseMembers.Add(dtoMember.Name);

                    if (!AreEnumValuesEqual(dtoMember.ConstantValue, baseMember.ConstantValue))
                    {
                        analysis.ValueMismatches.Add(
                            $"{dtoMember.Name} ({dtoMember.ConstantValue} != {baseMember.ConstantValue})"
                        );
                    }
                }
                else
                {
                    analysis.ExtraMembers.Add(dtoMember.Name);
                }
            }
        }

        foreach (var baseMember in baseMembers.Values)
        {
            if (!accountedBaseMembers.Contains(baseMember.Name))
            {
                analysis.MissingMembers.Add(baseMember.Name);
            }
        }

        return analysis;
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

    private static bool HasIgnoreEnumValueAttribute(IFieldSymbol field)
    {
        return field
            .GetAttributes()
            .Any(attr => attr.AttributeClass?.GetFullNamespaceName() == IgnoreEnumValueAttributeName);
    }

    private static AttributeData? GetEnumValueCorrespondsAttribute(IFieldSymbol field)
    {
        return field
            .GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.GetFullNamespaceName() == EnumValueCorrespondsAttributeName);
    }

    private static List<string> GetCorrespondingEnumNames(AttributeData correspondsAttr, INamedTypeSymbol baseEnum)
    {
        var names = new List<string>();

        for (var i = 0; i < correspondsAttr.ConstructorArguments.Length; i++)
        {
            var arg = correspondsAttr.ConstructorArguments[i];

            if (arg.Kind == TypedConstantKind.Array && arg.Values.Length > 0)
            {
                foreach (var value in arg.Values)
                {
                    if (value.Value is not null)
                    {
                        var memberName = GetEnumMemberNameByValue(baseEnum, value.Value);
                        if (memberName is not null)
                        {
                            names.Add(memberName);
                        }
                    }
                }
            }
            else if (arg.Value is not null)
            {
                var memberName = GetEnumMemberNameByValue(baseEnum, arg.Value);
                if (memberName is not null)
                {
                    names.Add(memberName);
                }
            }
        }

        return names;
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

    private sealed class EnumConsistencyAnalysis
    {
        public List<string> MissingMembers { get; } = [];
        public List<string> ExtraMembers { get; } = [];
        public List<string> ValueMismatches { get; } = [];

        public bool HasIssues => MissingMembers.Count > 0 || ExtraMembers.Count > 0 || ValueMismatches.Count > 0;
    }
}
