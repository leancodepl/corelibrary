using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using GreenPipes;
using LeanCode.Correlation;
using MassTransit;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public class CorrelationFilter : IFilter<ConsumeContext>
    {
        public void Probe(ProbeContext context)
        { }

        public async Task Send(ConsumeContext context, IPipe<ConsumeContext> next)
        {
            var correlationId = context.ConversationId is Guid conversationId ?
                Correlate.Logs(conversationId) :
                null;
            var consumerType = GetConsumerType(next);

            using (correlationId)
            using (consumerType)
            {
                await next.Send(context);
            }
        }

        private static IDisposable? GetConsumerType(IPipe<ConsumeContext> next)
        {
            var probe = next.GetProbeResult();

            if (!TryGetTyped<IDictionary<string, object>>(probe.Results, "filters", out var filters))
            {
                return null;
            }

            if (!TryGetTyped<IDictionary<string, object>>(filters, "consumer", out var consumer))
            {
                return null;
            }

            if (!TryGetTyped<string>(consumer, "type", out var type))
            {
                return null;
            }

            return Serilog.Context.LogContext.PushProperty("ConsumerType", type);
        }

        private static bool TryGetTyped<T>(IDictionary<string, object> dict, string key, [NotNullWhen(returnValue: true)] out T? value)
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

    public class CorrelationFilterPipeSpecification : IPipeSpecification<ConsumeContext>
    {
        public void Apply(IPipeBuilder<ConsumeContext> builder)
        {
            builder.AddFilter(new CorrelationFilter());
        }

        public IEnumerable<ValidationResult> Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }
    }

    public static class CorrelationFilterExtensions
    {
        public static void UseLogsCorrelation(this IPipeConfigurator<ConsumeContext> config) =>
            config.AddPipeSpecification(new CorrelationFilterPipeSpecification());
    }
}
