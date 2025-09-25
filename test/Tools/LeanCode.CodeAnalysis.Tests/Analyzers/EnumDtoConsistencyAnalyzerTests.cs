using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.Analyzers;

public class EnumDtoConsistencyAnalyzerTests : DiagnosticVerifier
{
    private const string AttributeDefinitions = """
        using System;

        namespace LeanCode.CodeAnalysis
        {
            [AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
            public sealed class IgnoreEnumDtoAttribute : Attribute;

            [AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
            public sealed class ExcludeMembersAttribute : Attribute
            {
                public object[] IgnoredValues { get; }
                public ExcludeMembersAttribute(params object[] ignoredValues)
                {
                    IgnoredValues = ignoredValues;
                }
            }

            [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
            public sealed class IgnoreEnumValueAttribute : Attribute;

            [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
            public sealed class EnumValueCorrespondsAttribute : Attribute
            {
                public object[] CorrespondingValues { get; }
                public EnumValueCorrespondsAttribute(params object[] correspondingValues)
                {
                    CorrespondingValues = correspondingValues;
                }
            }
        }
        """;

    [Fact]
    public async Task Matching_enum_dto_with_base_enum_should_pass()
    {
        var source = """
            namespace Test;

            public enum Status
            {
                None = 0,
                InProgress = 1,
                Completed = 2,
                Cancelled = 3
            }

            public enum StatusDTO
            {
                None = 0,
                InProgress = 1,
                Completed = 2,
                Cancelled = 3
            }
            """;

        await VerifyDiagnostics(source);
    }

    [Fact]
    public async Task Enum_dto_missing_members_should_fail()
    {
        var source = """
            namespace Test;

            public enum Status
            {
                None = 0,
                InProgress = 1,
                Completed = 2,
                Cancelled = 3
            }

            public enum StatusDTO
            {
                None = 0,
                InProgress = 1,
                Completed = 2
            }
            """;

        var diags = new[] { new DiagnosticResult(DiagnosticsIds.EnumDtoShouldMatchBaseEnum, 10, 12) };
        await VerifyDiagnostics(source, diags);
    }

    [Fact]
    public async Task Enum_dto_with_exclude_members_attribute_should_pass()
    {
        var source =
            AttributeDefinitions
            + """

                namespace Test;

                public enum Status
                {
                    None = 0,
                    InProgress = 1,
                    Completed = 2,
                    Cancelled = 3
                }

                [LeanCode.CodeAnalysis.ExcludeMembers(2, 3)]
                public enum StatusDTO
                {
                    None = 0,
                    InProgress = 1,
                }
                """;

        await VerifyDiagnostics(source);
    }

    [Fact]
    public async Task Enum_dto_with_extra_members_should_fail()
    {
        var source = """
            namespace Test;

            public enum Status
            {
                None = 0,
                InProgress = 1,
                Completed = 2,
                Cancelled = 3
            }

            public enum StatusDTO
            {
                None = 0,
                InProgress = 1,
                Completed = 2,
                Cancelled = 3,
                Deleted = 4
            }
            """;

        var diags = new[] { new DiagnosticResult(DiagnosticsIds.EnumDtoShouldMatchBaseEnum, 10, 12) };
        await VerifyDiagnostics(source, diags);
    }

    [Fact]
    public async Task Enum_dto_with_ignore_attribute_on_extra_member_should_pass()
    {
        var source =
            AttributeDefinitions
            + """

                namespace Test;

                public enum Status
                {
                    None = 0,
                    InProgress = 1,
                    Completed = 2,
                    Cancelled = 3
                }

                public enum StatusDTO
                {
                    None = 0,
                    InProgress = 1,
                    Completed = 2,
                    Cancelled = 3,
                    [LeanCode.CodeAnalysis.IgnoreEnumValue]
                    Deleted = 4
                }
                """;

        await VerifyDiagnostics(source);
    }

    [Fact]
    public async Task Enum_dto_with_value_mismatch_should_fail()
    {
        var source = """
            namespace Test;

            public enum Status
            {
                None = 0,
                InProgress = 1,
                Completed = 2,
                Cancelled = 3
            }

            public enum StatusDTO
            {
                None = 0,
                InProgress = 1,
                Completed = 2,
                Cancelled = 4
            }
            """;

        var diags = new[] { new DiagnosticResult(DiagnosticsIds.EnumDtoShouldMatchBaseEnum, 10, 12) };
        await VerifyDiagnostics(source, diags);
    }

    [Fact]
    public async Task Enum_dto_with_corresponds_attribute_should_pass()
    {
        var source =
            AttributeDefinitions
            + """

                namespace Test;

                public enum Status
                {
                    None = 0,
                    InProgress = 1,
                    Completed = 2,
                    Cancelled = 3
                }

                public enum StatusDTO
                {
                    None = 0,
                    [LeanCode.CodeAnalysis.EnumValueCorresponds(1)]
                    InProgress = 2,
                    [LeanCode.CodeAnalysis.EnumValueCorresponds(2)]
                    Completed = 3,
                    [LeanCode.CodeAnalysis.EnumValueCorresponds(3)]
                    Cancelled = 4
                }
                """;

        await VerifyDiagnostics(source);
    }

    [Fact]
    public async Task Enum_dto_with_value_mismatch_but_correct_corresponds_should_pass()
    {
        var source =
            AttributeDefinitions
            + """

                namespace Test;

                public enum Status
                {
                    None = 0,
                    InProgress = 1,
                    Completed = 2,
                    Cancelled = 3
                }

                public enum StatusDTO
                {
                    None = 0,
                    InProgress = 1,
                    [LeanCode.CodeAnalysis.EnumValueCorresponds(2)]
                    Completed = 3,
                    [LeanCode.CodeAnalysis.EnumValueCorresponds(3)]
                    Cancelled = 2
                }
                """;

        await VerifyDiagnostics(source);
    }

    [Fact]
    public async Task Enum_dto_with_multiple_corresponds_values_should_pass()
    {
        var source =
            AttributeDefinitions
            + """

                namespace Test;

                public enum Status
                {
                    None = 0,
                    InProgress = 1,
                    Completed = 2,
                    Cancelled = 3
                }

                public enum StatusDTO
                {
                    [LeanCode.CodeAnalysis.EnumValueCorresponds(0, 1)]
                    InProgress = 0,
                    [LeanCode.CodeAnalysis.EnumValueCorresponds(2)]
                    Completed = 1,
                    Cancelled = 3
                }
                """;

        await VerifyDiagnostics(source);
    }

    [Fact]
    public async Task Enum_dto_with_ignore_enum_dto_attribute_should_be_ignored()
    {
        var source =
            AttributeDefinitions
            + """

                namespace Test;

                public enum Status
                {
                    None = 0,
                    InProgress = 1,
                    Completed = 2,
                    Cancelled = 3
                }

                [LeanCode.CodeAnalysis.IgnoreEnumDto]
                public enum StatusDTO
                {
                    // This enum is completely different and should be ignored
                    Alpha = 100,
                    Beta = 200
                }
                """;

        await VerifyDiagnostics(source);
    }

    [Fact]
    public async Task Non_dto_enum_should_be_ignored()
    {
        var source = """
            namespace Test;

            public enum Status
            {
                None = 0,
                InProgress = 1,
                Completed = 2,
                Cancelled = 3
            }

            public enum Priority
            {
                Low = 1,
                Medium = 2,
                High = 3
            }
            """;

        await VerifyDiagnostics(source);
    }

    [Fact]
    public async Task Enum_dto_without_corresponding_base_enum_should_be_ignored()
    {
        var source = """
            namespace Test;

            public enum StatusDTO
            {
                None = 0,
                InProgress = 1,
                Completed = 2,
                Cancelled = 3
            }
            """;

        await VerifyDiagnostics(source);
    }

    protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
    {
        return new EnumDtoConsistencyAnalyzer();
    }
}
