using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenPipes;
using LeanCode.DomainModels.EventsExecution;
using MassTransit;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public class EventsPublisherFilter : IFilter<ConsumeContext>
    {
        public void Probe(ProbeContext context) { }

        public async Task Send(ConsumeContext context, IPipe<ConsumeContext> next)
        {
            var interceptor = context.GetService<AsyncEventsInterceptor>();
            interceptor.Prepare();

            await next.Send(context);

            var queue = interceptor.CaptureQueue();
            var publishTasks = queue.Select(evt => context.Publish((object)evt));
            await Task.WhenAll(publishTasks);
        }
    }

    public static class EventsPublisherFilterExtensions
    {
        public static void UseDomainEventsPublishing(this IPipeConfigurator<ConsumeContext> config) =>
            config.AddPipeSpecification(new EventsPublisherFilterPipeSpecification());
    }

    public class EventsPublisherFilterPipeSpecification : IPipeSpecification<ConsumeContext>
    {
        public void Apply(IPipeBuilder<ConsumeContext> builder) =>
            builder.AddFilter(new EventsPublisherFilter());

        public IEnumerable<ValidationResult> Validate() =>
            Enumerable.Empty<ValidationResult>();
    }
}
