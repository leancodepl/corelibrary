using System;
using System.Collections.Generic;
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
            var logs = context.ConversationId is Guid correlationId ?
                Correlate.Logs(correlationId) :
                null;

            using (logs)
            {
                await next.Send(context);
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