using System.Diagnostics.CodeAnalysis;
using MassTransit;

namespace LeanCode.CQRS.MassTransitRelay;

public static class ProbeSiteExtensions
{
    /// <remarks>
    /// Use only as last resort, i.e. you need consumer type before
    /// <see cref="MassTransit.ConsumerConsumeContext{TConsumer, TMessage}" /> is created
    /// </remarks>
    public static string? GetConsumerType(this IProbeSite pipe)
    {
        var probe = pipe.GetProbeResult();

        if (TryGetTyped(probe.Results, "consumer", out IDictionary<string, object>? consumer))
        {
            // short circuit if consumer is present at top level
            TryGetTyped<string>(consumer, "type", out var type);
            return type;
        }
        else
        {
            if (!TryGetTyped<IDictionary<string, object>>(probe.Results, "filters", out var filters))
            {
                return null;
            }

            if (!TryGetTyped(filters, "consumer", out consumer))
            {
                return null;
            }

            TryGetTyped<string>(consumer, "type", out var type);
            return type;
        }
    }

    private static bool TryGetTyped<T>(
        IDictionary<string, object> dict,
        string key,
        [NotNullWhen(returnValue: true)] out T? value
    )
        where T : class
    {
        if (dict.TryGetValue(key, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }
}
