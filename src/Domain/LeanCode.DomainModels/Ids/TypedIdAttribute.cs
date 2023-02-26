namespace LeanCode.DomainModels.Ids;

/// <summary>
/// The format of the ID that will be generated.
/// </summary>
/// <remarks>
/// See <see href="../../../../docs/domain/ids.md">documentation</see> for more details.
/// </remarks>
public enum TypedIdFormat : int
{
    /// <summary>
    /// Raw <see cref="int" />, without prefix. It's backing type is <see cref="int" />.
    /// </summary>
    RawInt = 0,

    /// <summary>
    /// Raw <see cref="long" />, without prefix. It's backing type is <see cref="int" />.
    /// </summary>
    RawLong = 1,

    /// <summary>
    /// Raw <see cref="Guid" />, without prefix. It's backing type is <see cref="int" />.
    /// </summary>
    RawGuid = 2,

    /// <summary>
    /// <see cref="Guid" /> prefixed with the class name or <see cref="TypedIdAttribute.CustomPrefix" />. It's backing
    /// type is <see cref="string" />.
    /// </summary>
    PrefixedGuid = 3,
}

/// <summary>
/// Attribute that mark <c>readonly partial record struct</c> for being <c>TypedId</c>, i.e. a stronly typed identifier
/// that will be generated during compilation and that can be used as a ID for
/// <see cref="Model.IIdentifiable{TIdentity}" /> and <see cref="Model.IAggregateRoot{TIdentity}" />.
/// </summary>
/// <remarks>
/// See <see href="../../../../docs/domain/ids.md">documentation</see> for more details.
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = true)]
public sealed class TypedIdAttribute : Attribute
{
    /// <summary>
    /// The format of the ID. <c>Prefixed</c> formats will respect <see cref="CustomPrefix" /> and
    /// <see cref="CustomGenerator" />. <c>Raw</c> formats will ignore them.
    /// </summary>
    public TypedIdFormat Format { get; }

    /// <summary>
    /// Custom prefix that will be used instead of class name for the ID value if the value will be prefixed.
    /// </summary>
    /// <remarks>
    /// The value should be lowercase. The generator will force it being lowercased.
    /// </remarks>
    public string? CustomPrefix { get; set; }

    /// <summary>
    /// Custom generator for the prefixed IDs.
    /// </summary>
    public string? CustomGenerator { get; set; }

    public TypedIdAttribute(TypedIdFormat format)
    {
        Format = format;
    }
}
