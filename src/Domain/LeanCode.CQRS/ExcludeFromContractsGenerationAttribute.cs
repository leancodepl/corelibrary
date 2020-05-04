using System;
using AT = System.AttributeTargets;

namespace LeanCode.CQRS
{
    [AttributeUsage(
        AT.Class | AT.Struct | AT.Enum | AT.Interface | AT.Property,
        AllowMultiple = false, Inherited = false)]
    public class ExcludeFromContractsGenerationAttribute : Attribute { }
}
