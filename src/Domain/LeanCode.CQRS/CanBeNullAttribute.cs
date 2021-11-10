using System;

namespace LeanCode.CQRS
{
    [Obsolete("Use C# 8.0's Nullable Reference Types language feature instead.")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class CanBeNullAttribute : Attribute { }
}
