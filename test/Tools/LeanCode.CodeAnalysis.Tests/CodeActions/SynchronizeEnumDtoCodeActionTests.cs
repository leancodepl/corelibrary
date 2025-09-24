using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.CodeFixProviders;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.CodeActions;

public class SynchronizeEnumDtoCodeActionTests : CodeFixVerifier
{
    private const string AttributeDefinitions =
        @"
using System;

namespace LeanCode.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public sealed class IgnoreEnumDtoAttribute : Attribute { }

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
    public sealed class IgnoreEnumValueAttribute : Attribute { }

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
";

    [Fact]
    public async Task Synchronizes_enum_dto_missing_members()
    {
        var source =
            @"
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
}";

        var expected =
            @"
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
}";

        var fixes = new[] { "Synchronize DTO enum with base enum" };
        await VerifyCodeFix(source, expected, fixes, 0);
    }

    [Fact]
    public async Task Synchronizes_enum_dto_with_value_mismatches()
    {
        var source =
            @"
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
    InProgress = 6,
    Completed = 7,
    Cancelled = 8
}";

        var expected =
            @"
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
}";

        var fixes = new[] { "Synchronize DTO enum with base enum" };
        await VerifyCodeFix(source, expected, fixes, 0);
    }

    [Fact]
    public async Task Removes_extra_members_but_keeps_those_with_ignore_attribute()
    {
        var source =
            AttributeDefinitions
            + @"

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
    ExtraWithoutAttribute = 4,
    [LeanCode.CodeAnalysis.IgnoreEnumValue]
    ExtraWithAttribute = 5
}";

        var expected =
            AttributeDefinitions
            + @"

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
    ExtraWithAttribute = 5
}";

        var fixes = new[] { "Synchronize DTO enum with base enum" };
        await VerifyCodeFix(source, expected, fixes, 0);
    }

    [Fact]
    public async Task Keeps_members_with_corresponds_attribute()
    {
        var source =
            AttributeDefinitions
            + @"

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
    [LeanCode.CodeAnalysis.EnumValueCorresponds(Status.InProgress)]
    InProgress = 2,
    [LeanCode.CodeAnalysis.EnumValueCorresponds(Status.Completed)]
    Completed = 3,
    [LeanCode.CodeAnalysis.EnumValueCorresponds(Status.Cancelled)]
    Cancelled = 4
}";

        await VerifyDiagnostics(source);
    }

    [Fact]
    public async Task Respects_exclude_members_attribute()
    {
        var source =
            AttributeDefinitions
            + @"

namespace Test;

public enum Status
{
    None = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

[LeanCode.CodeAnalysis.ExcludeMembers(Status.None, Status.InProgress)]
public enum StatusDTO
{
    WrongMember = 999,
    Completed = 2,
    Cancelled = 3
}";

        var expected =
            AttributeDefinitions
            + @"

namespace Test;

public enum Status
{
    None = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

[LeanCode.CodeAnalysis.ExcludeMembers(Status.None, Status.InProgress)]
public enum StatusDTO
{
    Completed = 2,
    Cancelled = 3
}";

        var fixes = new[] { "Synchronize DTO enum with base enum" };
        await VerifyCodeFix(source, expected, fixes, 0);
    }

    [Fact]
    public async Task Handles_complex_scenario_with_multiple_attributes()
    {
        var source =
            AttributeDefinitions
            + @"

namespace Test;

public enum Status
{
    None = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
    Archived = 4
}

[LeanCode.CodeAnalysis.ExcludeMembers(Status.Archived)]
public enum StatusDTO
{
    [LeanCode.CodeAnalysis.EnumValueCorresponds(Status.None, Status.InProgress)]
    InProgress = 0,
    [LeanCode.CodeAnalysis.EnumValueCorresponds(Status.Completed)]
    Completed = 1,
    Cancelled = 999,
    [LeanCode.CodeAnalysis.IgnoreEnumValue]
    CustomStatus = 100
}";

        var expected =
            AttributeDefinitions
            + @"

namespace Test;

public enum Status
{
    None = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
    Archived = 4
}

[LeanCode.CodeAnalysis.ExcludeMembers(Status.Archived)]
public enum StatusDTO
{
    [LeanCode.CodeAnalysis.EnumValueCorresponds(Status.None, Status.InProgress)]
    InProgress = 0,
    [LeanCode.CodeAnalysis.EnumValueCorresponds(Status.Completed)]
    Completed = 1,
    Cancelled = 3,
    [LeanCode.CodeAnalysis.IgnoreEnumValue]
    CustomStatus = 100
}";

        var fixes = new[] { "Synchronize DTO enum with base enum" };
        await VerifyCodeFix(source, expected, fixes, 0);
    }

    protected override CodeFixProvider GetCodeFixProvider()
    {
        return new SynchronizeEnumDtoCodeFixProvider();
    }

    protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
    {
        return new EnumDtoConsistencyAnalyzer();
    }
}
