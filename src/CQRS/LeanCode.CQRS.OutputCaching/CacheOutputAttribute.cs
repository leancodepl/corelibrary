namespace LeanCode.CQRS.OutputCaching;

// TODO: Port this to `LeanCode.Contracts` and use this attribute everywhere
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class CacheOutputAttribute : Attribute;
