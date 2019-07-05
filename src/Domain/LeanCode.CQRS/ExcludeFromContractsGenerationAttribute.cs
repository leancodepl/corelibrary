using System;

namespace LeanCode.CQRS
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface,
        AllowMultiple = false, Inherited = false)]
    public class ExcludeFromContractsGenerationAttribute : Attribute
    {
    }
}
