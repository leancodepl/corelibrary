using System;
using System.Reflection;

namespace LeanCode.CQRS
{
    /// <summary>
    /// Specifies that a contract property with a reference type is nullable.
    /// Used to facilitate contracts generation for languages with non-nullable refrence types, e.g TypeScript
    /// </summary>
    /// <remarks>
    /// Do not use this attribute for value types. Instead use standard <see cref="Nullable{T}" /> type.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class CanBeNullAttribute : Attribute
    {
    }
}
