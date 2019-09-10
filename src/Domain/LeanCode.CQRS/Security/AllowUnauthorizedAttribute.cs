using System;

namespace LeanCode.CQRS.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class AllowUnauthorizedAttribute : Attribute { }
}
