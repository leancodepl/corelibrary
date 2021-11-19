using System;

namespace LeanCode.CQRS.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class AllowUnauthorizedAttribute : Attribute { }
}
