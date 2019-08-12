using System;

namespace LeanCode.CQRS.Security
{
    /// <summary>
    /// Marker attribute for specifing that an object does not need authorization
    /// </summary>
    /// <remarks>
    /// This attribute does disable any authorization attribues, i.e if you specify both this attribute and
    /// <see cref="AuthorizeWhenAttribute" />, the latter will be taken into account. This attribute is
    /// used only by "LeanCode.CodeAnalysis" Roslyn analyzers project.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class AllowUnauthorizedAttribute : Attribute
    { }
}
