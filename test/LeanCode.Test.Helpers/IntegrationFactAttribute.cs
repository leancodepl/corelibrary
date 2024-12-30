using Xunit;
using Xunit.v3;

namespace LeanCode.Test.Helpers;

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
