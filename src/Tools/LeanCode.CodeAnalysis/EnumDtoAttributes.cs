namespace LeanCode.CodeAnalysis;

/// <summary>
/// Attribute that marks an enum ending with "DTO" to be completely ignored
/// by the enum DTO consistency analyzer.
/// </summary>
/// <remarks>
/// This attribute can only be applied to enums whose names end with "DTO".
/// When applied, the enum will not be checked for consistency with its base enum.
/// </remarks>
[AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
public sealed class IgnoreEnumDtoAttribute : Attribute;

/// <summary>
/// Attribute that specifies which enum values from the base enum should be excluded
/// when checking consistency with the DTO enum.
/// </summary>
/// <remarks>
/// This attribute can only be applied to enums whose names end with "DTO".
/// The excluded values are specified as parameters and correspond to values
/// from the base enum (without "DTO" suffix).
/// </remarks>
[AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
public sealed class ExcludeMembersAttribute : Attribute
{
    /// <summary>
    /// The enum values from the base enum that should be excluded from consistency checking.
    /// </summary>
    public object[] IgnoredValues { get; }

    /// <summary>
    /// Initializes a new instance of the ExcludeMembersAttribute class.
    /// </summary>
    /// <param name="ignoredValues">The enum values to exclude from consistency checking.</param>
    public ExcludeMembersAttribute(params object[] ignoredValues)
    {
        IgnoredValues = ignoredValues;
    }
}

/// <summary>
/// Attribute that marks a specific enum value in a DTO enum to be ignored
/// during consistency checking with the base enum.
/// </summary>
/// <remarks>
/// This attribute can only be applied to enum values within enums whose names end with "DTO".
/// When applied, the analyzer will not require a corresponding value in the base enum.
/// </remarks>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class IgnoreEnumValueAttribute : Attribute;

/// <summary>
/// Attribute that specifies which enum value(s) from the base enum this DTO enum value corresponds to.
/// </summary>
/// <remarks>
/// This attribute can only be applied to enum values within enums whose names end with "DTO".
/// When applied, the analyzer will check that this DTO enum value matches the specified base enum value(s)
/// instead of requiring a matching name and value.
/// </remarks>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class EnumValueCorrespondsAttribute : Attribute
{
    /// <summary>
    /// The enum values from the base enum that this DTO enum value corresponds to.
    /// </summary>
    public object[] CorrespondingValues { get; }

    /// <summary>
    /// Initializes a new instance of the EnumValueCorrespondsAttribute class.
    /// </summary>
    /// <param name="correspondingValues">The base enum values this DTO enum value corresponds to.</param>
    public EnumValueCorrespondsAttribute(params object[] correspondingValues)
    {
        CorrespondingValues = correspondingValues;
    }
}
