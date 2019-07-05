using System;

namespace LeanCode.CQRS
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class CanBeNullAttribute : Attribute
    {
    }
}
