using System.Reflection;
using Xunit;
using Xunit.v3;

namespace LeanCode.Test.Helpers;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public abstract class ExternalServiceFactAttribute : FactAttribute, ITraitAttribute, IBeforeAfterTestAttribute
{
    protected abstract IReadOnlyCollection<string> RequiredEnvVariables { get; }
    protected abstract string ServiceType { get; }

    public ExternalServiceFactAttribute()
    {
        Explicit = true;
    }

    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
        [new("category", "external"), new("service", ServiceType), new("integration", "true")];

    public void After(MethodInfo methodUnderTest, IXunitTest test) { }

    public void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        var skipReason = SkipIfVariablesNotSet(RequiredEnvVariables.ToHashSet());

        if (skipReason != null)
        {
            Assert.Fail(skipReason);
        }
    }

    private static string SkipIfVariablesNotSet(IReadOnlySet<string> variables)
    {
        return variables.Any(v => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(v)))
            ? $"Set `{string.Join(",", variables)}` env variables first"
            : null;
    }
}
