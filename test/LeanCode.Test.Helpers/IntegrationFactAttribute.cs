using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.v3;

namespace LeanCode.Test.Helpers;

[SuppressMessage("?", "CA1813", Justification = "The attribute is inherited by other attributes and cannot be sealed.")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class IntegrationFactAttribute : FactAttribute, ITraitAttribute
{
    public IntegrationFactAttribute()
    {
        Explicit = true;
    }

    public virtual IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
        [new("category", "integration"), new("integration", "true")];
}
